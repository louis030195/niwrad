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
	creator   string
	matchID   string
	presences map[string]c.Presence
}

type Match struct{}

func (m *Match) MatchInit(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, params map[string]interface{}) (interface{}, int, string) {
	logger.Info("params %v", params)
	creator, ok := params["creator"].(string)
	if !ok {
		logger.Error("Error creating match, no creator in params")
		return nil, 0, ""
	}
	state := &MatchState{
		creator:   creator,
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

	// var rtapiPresences []*rtapi.UserPresence

	for _, p := range presences {
		mState.presences[p.GetUserId()] = p
		// rtapiPresences = append(rtapiPresences, &rtapi.UserPresence{
		// 	UserId:      p.GetUserId(),
		// 	SessionId:   p.GetSessionId(),
		// 	Username:    p.GetUsername(),
		// 	Persistence: p.GetPersistence(),
		// })
	}
	// Notify everyone of this player arrival
	// matchPresence, err := proto.Marshal(&rtapi.MatchPresenceEvent{
	// 	MatchId: mState.matchID,
	// 	Joins:   rtapiPresences,
	// })
	// if err != nil {
	// 	return err
	// }
	// dispatcher.BroadcastMessage(1, matchPresence, nil, nil, true)

	return mState
}

func (m *Match) MatchLeave(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchLeave, %v", presences)
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		delete(mState.presences, p.GetUserId())
		if p.GetUserId() == mState.creator {
			mState.creator = ""
			// return nil // end match
		}
	}

	return mState
}

func (m *Match) MatchLoop(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, messages []c.MatchData) interface{} {
	mState, _ := state.(*MatchState)
	for _, message := range messages {
		// logger.Info("Received %v from %v", string(message.GetData()), message.GetUserId())

		// Don't send to sender
		others := []c.Presence{message}
		idx := -1
		for i := range others {
			if others[i].GetUserId() == message.GetUserId() {
				idx = i
				break
			}
		}
		// Should panic if idx == -1 (sender isn't in presences ???)
		others = append(others[:idx], others[idx+1:]...)

		dispatcher.BroadcastMessage(1, message.GetData(), others, nil, true)
	}

	// Stop match if empty after a while
	if tick > tickRate*20 && len(mState.presences) == 0 {
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
