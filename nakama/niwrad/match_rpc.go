package main

import (
	"context"
	"database/sql"
	"encoding/json"
	"fmt"
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/rpc"
)

var (
	// Error responses
	errBadContext    = runtime.NewError("bad context", 3)
	errMarshal       = runtime.NewError("cannot marshal response", 13)
	errUnmarshal     = runtime.NewError("cannot unmarshal request", 13)
	errGetAccount    = runtime.NewError("cannot find account", 14)
	errUpdateAccount = runtime.NewError("cannot update account", 14)
	errGetServers    = runtime.NewError("cannot find servers", 15)
	errStopServer    = runtime.NewError("cannot stop server", 16) // TODO: better errors
)

type sessionContext struct {
	UserID    string
	SessionID string
}

func unpackContext(ctx context.Context) (*sessionContext, error) {
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	return &sessionContext{UserID: userID, SessionID: sessionID}, nil
}
func rpcCreateMatch(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	var request rpc.CreateMatchRequest
	if err = proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	// Create the party match.
	matchID, err := nk.MatchCreate(ctx, "Niwrad", map[string]interface{}{
		"creator": session.UserID,
		"workerId": request.WorkerId,
	})

	if err != nil {
		logger.Error("match created failed: %v", err)
		response := &rpc.CreateMatchResponse{
			Result: rpc.CreateMatchCompletionResult_createMatchCompletionResultUnknownInternalFailure,
		}
		responseBytes, err := proto.Marshal(response)
		if err != nil {
			return "", errMarshal
		}
		return string(responseBytes), nil
	}

	// Return result to user.
	response := &rpc.CreateMatchResponse{
		MatchId: matchID,
		Result:  rpc.CreateMatchCompletionResult_createMatchCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

// Request to create a Unity server, should check if user is allowed to do that
// If yes, spawn the server either as a k8s deployment, either a simple process
// with the config and store the server (with some ID and other infos) in the user account
func rpcRunUnityServer(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	user, userStorage, err := getUserInfo(ctx, nk, session.UserID)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}
	servers, err := getServers(ctx, nk, userStorage.ServersWorkerId)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}

	var request rpc.RunServerRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	logger.Info("A server creation has been asked by {\"username\": %s, \"storage\":%v, \"servers\":%v} with config: %v",
		user.Username,
		userStorage,
		servers,
		request)

	if request.Configuration.TerrainSize < 1 {
		request.Configuration.TerrainSize = 1000
	}
	if request.Configuration.InitialAnimals < 1 {
		request.Configuration.InitialAnimals = 50
	}
	if request.Configuration.InitialPlants < 1 {
		request.Configuration.InitialPlants = 100
	}

	unityDeploymentId := fmt.Sprintf("%s-%d", session.UserID, len(*servers))
	res, err := spawnUnityPod(unityDeploymentId, *request.Configuration)
	if err != nil {
		logger.Error(err.Error())
		return "", nil
	}

	server := rpc.UnityServer{MatchId: *res, Configuration: request.Configuration}
	logger.Info("New server %v", server)

	jsonServer, err := json.Marshal(server)
	if err != nil {
		logger.Error("Failed to marshal server")
		return "", errMarshal
	}
	objects := []*runtime.StorageWrite{
		{
			Collection:      "server",
			Key:             unityDeploymentId,
			UserID:          session.UserID,
			Value:           string(jsonServer),
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		logger.Error("Failed to write server to storage: %s", err.Error())
		return "", errUpdateAccount
	}
	if userStorage != nil { // This user is already in storage
		userStorage.ServersWorkerId = append(userStorage.ServersWorkerId, *res)
	} else {
		userStorage = &rpc.User{ServersWorkerId: []string{*res}}
	}
	jsonUser, err := json.Marshal(userStorage)
	if err != nil {
		logger.Error("Failed to marshal user")
		return "", errMarshal
	}
	objects = []*runtime.StorageWrite{
		{
			Collection:      "user",
			Key:             session.UserID,
			UserID:          session.UserID,
			Value:           string(jsonUser),
			PermissionRead:  2,
			PermissionWrite: 1,
		},
	}

	if _, err := nk.StorageWrite(ctx, objects); err != nil {
		logger.Error("Storage update error: %s", err.Error())
		return "", errUpdateAccount
	}

	// Return result to user.
	response := &rpc.RunServerResponse{
		Result: rpc.RunServerCompletionResult_runServerCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

func rpcStopUnityServer(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	user, userStorage, err := getUserInfo(ctx, nk, session.UserID)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}
	servers, err := getServers(ctx, nk, userStorage.ServersWorkerId)
	if err != nil {
		logger.Error("failed to get user info")
		return "", err
	}

	var request rpc.StopServerRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	logger.Info("A server deletion has been asked by {\"username\": %s, \"storage\":%v, \"servers\":%v} with config: %v",
		user.Username,
		userStorage,
		servers,
		request)

	var workerId string
	// We need to retrieve the server filtering by match id (e.g. select * from servers S where S.matchId = myMatchId)
	for _, s := range *servers {
		if s.MatchId == request.MatchId {
			workerId = s.MatchId
		}
	}
	if workerId == "" {
		logger.Error(errStopServer.Error())
		return "", errStopServer
	}

	// Stop the k8s deployment
	err = stopUnityDeployment(workerId)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}
	objects := []*runtime.StorageDelete{
		{
			Collection:      "user",
			Key:             workerId,
			UserID:          session.UserID,
		},
	}
	// Delete server from storage
	if err = nk.StorageDelete(ctx, objects); err != nil {
		logger.Error(err.Error())
		return "", err
	}
	// Return result to user.
	response := &rpc.StopServerResponse{
		Result: rpc.StopServerCompletionResult_stopServerCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}
