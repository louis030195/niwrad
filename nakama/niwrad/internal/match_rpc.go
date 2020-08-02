package niwrad

import (
	"context"
	"database/sql"
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/api/rpc"
	"github.com/louis030195/niwrad/internal/storage"
)

var (
	// Error responses
	errBadContext    = runtime.NewError("bad context", 3)
	errMarshal       = runtime.NewError("cannot marshal response", 13)
	errUnmarshal     = runtime.NewError("cannot unmarshal request", 13)
	errUpdateAccount = runtime.NewError("cannot update account", 14)
	errStopServer    = runtime.NewError("cannot stop server", 16) // TODO: better errors
)

type sessionContext struct {
    Username string
	UserID    string
	SessionID string
}

func unpackContext(ctx context.Context) (*sessionContext, error) {
    username, ok := ctx.Value(runtime.RUNTIME_CTX_USERNAME).(string)
    if !ok {
        return nil, errBadContext
    }
	userID, ok := ctx.Value(runtime.RUNTIME_CTX_USER_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	sessionID, ok := ctx.Value(runtime.RUNTIME_CTX_SESSION_ID).(string)
	if !ok {
		return nil, errBadContext
	}
	return &sessionContext{Username: username, UserID: userID, SessionID: sessionID}, nil
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

	users, usersStorage, err := storage.GetUsers(ctx, nk, session.UserID)
	if err != nil {
		logger.Error("failed to get user info %v", err)
		return "", err
	}
	if len(usersStorage) > 1 {
		err = runtime.NewError("duplicated user storage !", 99) // Impossible ?
		logger.Error(err.Error())
		return "", err
	}

	var request rpc.RunServerRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}

	if len(usersStorage) == 1 { // TODO: right to create server
		logger.Info("A server creation has been asked by username: %s, storage:%v with config: %v",
			users[0].Username,
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
	response := &rpc.RunServerResponse{
		Result: rpc.RunServerCompletionResult_runServerCompletionResultSucceeded,
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

	users, usersStorage, err := storage.GetUsers(ctx, nk, session.UserID)
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
		err = runtime.NewError("unknown user !", 45)
		logger.Error(err.Error())
		return "", err
	}
	servers, err := storage.GetServers(ctx, nk, usersStorage[0].MatchesOwned...)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}

	var request rpc.StopServerRequest
	if err := proto.Unmarshal([]byte(payload), &request); err != nil {
		logger.Error("unmarshalling failed: %v %v", payload, err)
		return "", errUnmarshal
	}
	logger.Info("A server deletion has been asked by {\"username\": %s, \"storage\":%v, \"servers\":%v} with config: %v",
		users[0].Username,
		usersStorage[0],
		servers,
		request)

	// Stop the k8s deployment
	err = stopMatch(request.MatchId)
	if err != nil {
		logger.Error(err.Error())
		return "", err
	}
	// TODO: Do we want to erase it ? or keep for later restart ?
	//if err := storage.DeleteServer(ctx, nk, request.MatchId, session.UserID); err != nil {
	//    logger.Error(err.Error())
	//    return "", err
	//}

	// Once server removed from server table,
	// should we remove it or maybe user want to restart the server with saved state ?
	//matchesOwned := usersStorage[0].MatchesOwned
	//for i := range matchesOwned {
	//    if matchesOwned[i] == request.MatchId {
	//        matchesOwned[len(matchesOwned)-1], matchesOwned[i] = matchesOwned[i], matchesOwned[len(matchesOwned)-1]
	//    }
	//}
	//if err := storage.UpdateUser(ctx, nk, session.UserID, matchesOwned); err != nil {
	//    logger.Error(err.Error())
	//    return "", err
	//}
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
