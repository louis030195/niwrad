package main

import (
	"context"
	"database/sql"
	"strconv"

	c "github.com/heroiclabs/nakama-common/runtime"
)

var (
	tickRate int64 = 5
)

type MatchState struct {
	matchID   string
	presences map[string]c.Presence
}

type Match struct{}

func (m *Match) MatchInit(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, params map[string]interface{}) (interface{}, int, string) {
	logger.Info("MatchInit, params: %v", params)
	state := &MatchState{
		matchID:   ctx.Value(c.RUNTIME_CTX_MATCH_ID).(string),
		presences: make(map[string]c.Presence),
	}
	label := ""
	return state, int(tickRate), label
}

func (m *Match) MatchJoinAttempt(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presence c.Presence, metadata map[string]string) (interface{}, bool, string) {
	logger.Info("MatchJoinAttempt, %v", presence)
	acceptUser := true
	return state, acceptUser, ""
}

func (m *Match) MatchJoin(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchJoin, %v", presences)
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		mState.presences[p.GetUserId()] = p
	}

	return mState
}

func (m *Match) MatchLeave(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchLeave, %v", presences)
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		delete(mState.presences, p.GetUserId())
	}

	return mState
}

func (m *Match) MatchLoop(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, messages []c.MatchData) interface{} {
	mState, _ := state.(*MatchState)
	for _, message := range messages {
		logger.Info("Received %v from %v", string(message.GetData()), message.GetUserId())

		dispatcher.BroadcastMessage(1, message.GetData(), []c.Presence{message}, nil, true)
	}

	// Need to check tick otherwise will run loop before creator join
	if tick > tickRate*10 && len(mState.presences) == 0 {
		logger.Info("Match %v is empty, terminating it", mState.matchID)
		// Terminate match when empty
		m.MatchTerminate(ctx, logger, db, nk, dispatcher, tick, state, 2)
		return nil
	}

	return mState
}

func (m *Match) MatchTerminate(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, graceSeconds int) interface{} {
	logger.Info("MatchTerminate")
	message := "Server shutting down in " + strconv.Itoa(graceSeconds) + " seconds."
	dispatcher.BroadcastMessage(2, []byte(message), nil, nil, true)
	return state
}
