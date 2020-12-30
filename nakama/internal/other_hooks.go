package niwrad

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/api"
	"github.com/heroiclabs/nakama-common/runtime"
)

func AfterAuthenticateEmail(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, out *api.Session, in *api.AuthenticateEmailRequest) error {
	logger.Info("AfterAuthenticateEmail by %v", in.Account)
	return nil
}

func AfterAuthenticateDevice(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, out *api.Session, in *api.AuthenticateDeviceRequest) error {
    logger.Info("AfterAuthenticateDevice by %v", in.Account)
    id := "naive"
    sort := "desc"
    operator := "best"
    reset := "0 0 * * 1"
    metadata := map[string]interface{}{"weather_conditions": "rain"}
    if err := nk.LeaderboardCreate(ctx, id, true, sort, operator, reset, metadata); err != nil {
        // Probably already exists
        logger.Info("couldn't create leaderboard")
    }
    return nil
}

func AfterWriteLeaderboardRecord(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule,
    out *api.LeaderboardRecord, in *api.WriteLeaderboardRecordRequest) error {
    logger.Info("AfterWriteLeaderboardRecord %v", in.Record)
    return nil
}

func AfterListLeaderboardRecords(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule,
    out *api.LeaderboardRecordList, in *api.ListLeaderboardRecordsRequest) error {
    logger.Info("AfterListLeaderboardRecords, count %v", len(out.Records))
    return nil
}

// TODO: before write storage objects: validation deep learning Pod injurious name detector :D

func AfterWriteStorageObjects(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule,
    out *api.StorageObjectAcks, in *api.WriteStorageObjectsRequest) error {
    logger.Info("AfterWriteStorageObjects, count %v", len(out.Acks))
    return nil
}

func AfterListStorageObjects(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule,
    out *api.StorageObjectList, in *api.ListStorageObjectsRequest) error {
    logger.Info("AfterListStorageObjects, count %v", len(out.Objects))
    return nil
}
