package main

import (
	"context"
	"database/sql"
	"niwrad/realtime"
	"strconv"

	"github.com/golang/protobuf/proto"
	c "github.com/heroiclabs/nakama-common/runtime"
)

var (
	tickRate int64 = 30
)

type MatchState struct {
	creator   c.Presence
	matchID   string
	presences map[string]c.Presence
}

type Match struct{}

func (m *Match) MatchInit(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, params map[string]interface{}) (interface{}, int, string) {
	state := &MatchState{
		matchID:   ctx.Value(c.RUNTIME_CTX_MATCH_ID).(string),
		presences: make(map[string]c.Presence),
	}
	logger.Info("MatchInit, params: %v, state: %v", params, state)

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
		if len(mState.presences) == 0 {
			mState.creator = p
		}
		mState.presences[p.GetUserId()] = p
	}
	return mState
}

func (m *Match) MatchLeave(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchLeave, %v", presences)
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		delete(mState.presences, p.GetUserId())
		if p.GetUserId() == mState.creator.GetUserId() {
			return nil // end match
		}
	}

	return mState
}

func (m *Match) MatchLoop(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, messages []c.MatchData) interface{} {
	mState, _ := state.(*MatchState)
	for _, message := range messages {
		var s realtime.Packet
		if err := proto.Unmarshal(message.GetData(), &s); err != nil {
			logger.Error("Failed to parse match packet:", err)
		}

		// No recipients set, "nil" = send to everyone
		// if len(s.Recipients) == 0 {
		// 	dispatcher.BroadcastMessage(1, message.GetData(), nil, nil, true)
		// 	continue
		// }
		for _, r := range s.Recipients {
			presence, ok := mState.presences[r]
			if !ok {
				logger.Error("Tried to send message to in-existent player")
			}
			logger.Info("Sending message to %v - %v", r, s.Type)
			dispatcher.BroadcastMessage(1, message.GetData(), []c.Presence{presence}, nil, true)
		}
	}

	// Stop match if empty after a while
	if tick > tickRate*3 && len(mState.presences) == 0 {
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
