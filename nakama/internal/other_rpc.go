package niwrad

import (
	"context"
	"database/sql"
    "github.com/golang/protobuf/proto"
    "github.com/heroiclabs/nakama-common/runtime"
    "github.com/louis030195/niwrad/api/rpc"
    "github.com/louis030195/niwrad/internal/storage"
)

//// RpcDeleteAllAccounts delete all Nakama accounts, TODO: limit a bit so that not everyone can do that :)
func RpcDeleteAllAccounts(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	if err := storage.DeleteAll(db, "collection = 'user'"); err != nil {
	    return "", err
    }
	logger.Info("Deleted all accounts")
	return "", nil
}

func RpcSendLeaderboard(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
    var session *sessionContext
    var err error
    if session, err = unpackContext(ctx); err != nil {
        logger.Error("unpack context failed: %v", err)
        return "", errBadContext
    }

    var request rpc.NaiveLeaderboardRequest
    if err := proto.Unmarshal([]byte(payload), &request); err != nil {
        logger.Error("unmarshalling failed: %v %v", payload, err)
        return "", errUnmarshal
    }
    record, err := nk.LeaderboardRecordWrite(ctx, "naive", session.UserID, session.Username, request.Hosts, 0,
        map[string]interface{}{})
    if err != nil {
        logger.Error("nk.LeaderboardRecordWrite failed: %v %v", payload, err)
        return "", errUnmarshal
    }
    logger.Info("AfterWriteLeaderboardRecord %v", request.Hosts)
    responseBytes, err := proto.Marshal(record)
    if err != nil {
        logger.Error("marshalling failed: %v %v", payload, err)
        return "", errMarshal
    }
    return string(responseBytes), nil
}

//func RpcSendExperience(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
//
//}
