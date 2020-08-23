package niwrad

import (
	"context"
	"database/sql"
	"fmt"
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/api/rpc"
	"github.com/louis030195/niwrad/internal/storage"
	"math"
)

var (
	// Error responses
	errBadContext    = runtime.NewError("bad context", 3)
	errMarshal       = runtime.NewError("cannot marshal response", 13)
	errUnmarshal     = runtime.NewError("cannot unmarshal request", 13)
	errUpdateAccount = runtime.NewError("cannot update account", 14)
	errStopServer    = runtime.NewError("cannot stop server", 16) // TODO: better errors
)

const (
	READY_LABEL = "ready"
)

type sessionContext struct {
	Username  string
	UserID    string
	SessionID string
}

func unpackContext(ctx context.Context) (*sessionContext, error) {
	username, ok := ctx.Value(runtime.RUNTIME_CTX_USERNAME).(string)
	if !ok {
		err := errBadContext
		err.Message = "RUNTIME_CTX_USERNAME " + err.Message
		return nil, err
	}
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		err := errBadContext
		err.Message = "RUNTIME_CTX_USER_ID " + err.Message
		return nil, err
	}
	sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	if !ok {
		err := errBadContext
		err.Message = "RUNTIME_CTX_SESSION_ID " + err.Message
		return nil, err
	}
	return &sessionContext{Username: username, UserID: userID, SessionID: sessionID}, nil
}

// RpcListMatches
func RpcListMatches(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	min := 0
	max := math.MaxInt64
	// Only return ready-state matches (all executors joined & are ready)
	matches, err := nk.MatchList(ctx, math.MaxInt64, true, READY_LABEL, &min, &max, "")
	var matchesId []string
	for _, m := range matches {
		matchesId = append(matchesId, m.MatchId)
	}
	// Return result to user.
	response := &rpc.ListMatchesResponse{
		MatchesId: matchesId,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

// RpcCreateMatch Client request for match creation, checking if allowed and if yes creating a match with x containers
// And updating storage accordingly
func RpcCreateMatch(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error("unpack context failed: %v", err)
		return "", errBadContext
	}

	usersStorage, err := storage.GetUsers(ctx, nk, session.UserID)
	if err != nil {
		logger.Error("failed to get user info %v", err)
		return "", err
	}
	if len(usersStorage) > 1 {
		err = runtime.NewError("duplicated user storage !", 99) // Impossible ?
		logger.Error(err.Error())
		return "", err
	}

	var request rpc.CreateMatchRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}

	if len(usersStorage) == 1 { // TODO: right to create server
		logger.Info("A server creation has been asked by user:%v with config: %v",
			usersStorage[0],
			request)
	}
	matchId, err := startMatch(ctx, nk, session.UserID, 4)
	if err != nil {
		logger.Error(err.Error())
		return "", nil
	}

	var matchesOwned []string
	if len(usersStorage) == 1 { // This user is already in storage
		matchesOwned = append(usersStorage[0].MatchesOwned, matchId)
	} else {
		matchesOwned = []string{matchId}
	}

	if err := storage.UpdateUser(ctx, nk, session.UserID, matchesOwned); err != nil {
		logger.Error(err.Error())
		return "", err
	}

	// Return result to user.
	response := &rpc.CreateMatchResponse{
		MatchId: matchId,
		Result:  rpc.CreateMatchCompletionResult_createMatchCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}

func RpcStopMatch(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	var session *sessionContext
	var err error
	if session, err = unpackContext(ctx); err != nil {
		logger.Error(err.Error())
		return "", errBadContext
	}

	usersStorage, err := storage.GetUsers(ctx, nk, session.UserID)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}
	if len(usersStorage) > 1 {
		err = runtime.NewError("duplicated user storage !", 99) // Impossible ?
		logger.Error(err.Error())
		return "", err
	}
	if len(usersStorage) == 0 {
		err = runtime.NewError(fmt.Sprintf("unknown user %s", session.UserID), 45)
		logger.Error(err.Error())
		return "", err
	}
	user := usersStorage[0]
	servers, err := storage.GetMatches(ctx, nk, user.MatchesOwned...)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}

	var request rpc.StopMatchRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	logger.Info("A server deletion has been asked by {\"user\":%v, \"servers\":%v} with config: %v",
		user,
		servers,
		request)

	// Stop the k8s deployment
	err = stopMatch(ctx, nk, request.MatchId)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}

	// Once server removed from server table,
	// should we remove it or maybe user want to restart the server with saved state ?
	matchesOwned := user.MatchesOwned
	for i := range matchesOwned {
		if matchesOwned[i] == request.MatchId {
			matchesOwned[len(matchesOwned)-1], matchesOwned[i] = matchesOwned[i], matchesOwned[len(matchesOwned)-1]
		}
	}
	// Removing the deleted match from user storage
	if err := storage.UpdateUser(ctx, nk, user.Id, matchesOwned); err != nil {
		return "", err
	}

	// Return result to user.
	response := &rpc.StopMatchResponse{
		Result: rpc.StopMatchCompletionResult_stopServerCompletionResultSucceeded,
	}
	responseBytes, err := proto.Marshal(response)
	if err != nil {
		return "", errMarshal
	}
	return string(responseBytes), nil
}
