package niwrad

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
)

//// RpcDeleteAllAccounts delete all Nakama accounts, TODO: limit a bit so that not everyone can do that :)
func RpcDeleteAllAccounts(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, payload string) (string, error) {
	_, err := db.Exec("DELETE FROM users")
	if err != nil {
		return "", err
	}
	logger.Info("Deleted all accounts")
	return "", nil
}
