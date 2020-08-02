package main

import (
	"context"
	"database/sql"
	"github.com/heroiclabs/nakama-common/runtime"
	"github.com/louis030195/niwrad/internal"
)

const (
	serviceName = "Niwrad"
)

// Register the collection of functions with Nakama required to provide an OnlinePartyService from Unreal Engine.
func Register(initializer runtime.Initializer) error {
	createPartyMatch := func(ctx context.Context, logger runtime.Logger, db *sql.DB, nk runtime.NakamaModule) (runtime.Match, error) {
		return &niwrad.Match{}, nil
	}
	if err := initializer.RegisterMatch(serviceName, createPartyMatch); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("create_match", niwrad.RpcCreateMatch); err != nil {
		return err
	}
	if err := initializer.RegisterRpc("stop_match", niwrad.RpcStopMatch); err != nil {
		return err
	}
	if err := initializer.RegisterAfterAuthenticateEmail(niwrad.AfterAuthenticateEmail); err != nil {
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
