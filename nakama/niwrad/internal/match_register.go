package niwrad

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
)

const (
	serviceName = "Niwrad"
)

// Register the collection of functions with Nakama required to provide an OnlinePartyService from Unreal Engine.
func Register(initializer runtime.Initializer) error {
	createPartyMatch := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule) (runtime.Match, error) {
		return &Match{}, nil
	}
	if err := initializer.RegisterMatch(serviceName, createPartyMatch); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("create_match", rpcCreateMatch); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("stop_match", rpcStopMatch); err != nil {
		return err
	}
	if err := initializer.RegisterAfterAuthenticateEmail(afterAuthenticateEmail); err != nil {
		return err
	}
	return nil
}

func InitModule(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule, initializer runtime.Initializer) error {
	if err := Register(initializer); err != nil {
		return err
	}
	logger.Info("Registered hooks successfully")
	return nil
}
