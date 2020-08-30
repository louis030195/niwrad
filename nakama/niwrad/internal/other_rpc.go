package niwrad

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
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
