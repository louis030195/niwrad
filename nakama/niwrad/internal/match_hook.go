package niwrad

import (
	"context"
	"database/sql"
	"github.com/louis030195/niwrad/api/realtime"
	"github.com/louis030195/niwrad/internal/gen"
	octree "github.com/louis030195/octree/pkg"
    "github.com/louis030195/protometry/pkg/vector3"
    "github.com/louis030195/protometry/pkg/volume"
	"math"
	"strconv"

	"github.com/golang/protobuf/proto"
	c "github.com/heroiclabs/nakama-common/runtime"
)

var (
	tickRate        int64 = 30
	whitelistAdmins       = []string{"bbbb@bbbb.com"}
)

type PresenceState struct {
	region   *volume.Box
	presence c.Presence
}

type MatchState struct {
	matchID      string
	presences    map[string]PresenceState
	octree       *octree.Octree
	servers      []*volume.Box
	distribution int64
	m            *realtime.Matrix
	spawned      int
}

type Match struct{}

func (m *Match) MatchInit(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, params map[string]interface{}) (interface{}, int, string) {
	generatedMap, err := gen.DiamondSquare(int(math.Pow(2, 8)+1), 40, 1)

	if err != nil {
		logger.Error(err.Error())
		return nil, 0, ""
	}

	matchID := ctx.Value(c.RUNTIME_CTX_MATCH_ID).(string)
	distribution, ok := params["distribution"].(int64)
	if !ok {
		logger.Error("Failed to init match")
		if err := stopMatch(matchID); err != nil {
			logger.Error(err.Error())
			return nil, 0, ""
		}
		return nil, 0, ""
	}
	size := 1000.
	region := volume.NewBoxOfSize(0, 0, 0, size)
	oc := octree.NewOctree(region)
	state := &MatchState{
		matchID:      matchID,
		presences:    make(map[string]PresenceState),
		octree:       oc,
		distribution: distribution,
		m:            generatedMap,
	}
	logger.Info("MatchInit, params: %v, state: %v", params, state)
	return state, int(tickRate), ""
}

func (m *Match) MatchJoinAttempt(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presence c.Presence, metadata map[string]string) (interface{}, bool, string) {
	logger.Info("MatchJoinAttempt, %v", presence)
	mState, _ := state.(*MatchState)
	b := volume.NewBoxOfSize(0, 0, 0, 50)
	for i := range whitelistAdmins {
		if presence.GetUsername() == whitelistAdmins[i] {
			bs := volume.NewBoxOfSize(0, 0, 0, float64(mState.octree.GetSize())).Split()
			// hard coded 4 distribution atm
			// meaning that each container handle 2 octant
			b = bs[len(mState.servers)].EncapsulateBox(*bs[len(mState.servers)+1])
			mState.servers = append(mState.servers, b)
			logger.Info("New match server, region: %v ", b)
			break
		}
	}
	mState.presences[presence.GetUserId()] = PresenceState{presence: presence, region: b}
	if err := realtime.Send(dispatcher, []c.Presence{presence}, &realtime.Packet_MatchJoin{
		MatchJoin: &realtime.MatchJoin{
			Information: &realtime.MatchInformation{
				Region: b,
				Map:    mState.m,
			},
		},
	}); err != nil {
		logger.Error(err.Error())
		return nil, false, ""
	}
	return state, true, ""
}

func (m *Match) MatchJoin(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchJoin, %v", presences)
	return state
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
		var s realtime.Packet
		if err := proto.Unmarshal(message.GetData(), &s); err != nil {
			logger.Error("Failed to parse match packet:", err)
		}

		// By default clients / server will send to all recipients (except for some special packets)
		// They are not aware that nakama server will only send to recipients concerned (in the region of impact)
		for _, r := range s.Recipients {
			presence, ok := mState.presences[r]
			if !ok {
				logger.Error("Tried to send message to in-existent player")
				continue
			}

			// If the recipient should be impacted
			// nil impact = global
			if s.Impact == nil || presence.region.Contains(*s.Impact) {
				logger.Info("Sending message to %v - %v", r, s.Type)
				err := dispatcher.BroadcastMessage(1, message.GetData(), []c.Presence{presence.presence}, nil, true)
				if err != nil {
					logger.Error(err.Error())
					return err
				}
			}
		}
	}

	// Stop match if empty after a while
	if tick > tickRate*3 && len(mState.presences) == 0 {
		logger.Info("Match %v is empty, terminating it", mState.matchID)
		// Terminate match when empty
		m.MatchTerminate(ctx, logger, db, nk, dispatcher, tick, state, 2)
		return nil
	}

	if len(mState.servers) == 4 { // When all servers joined
	    for i := 0; i < 5; i++ {
	        // I suppose server handling that request will do the work to adjust above ground by ray-casting
	        // avoid like the plague physics here :)
            randomPos := vector3.RandomSpherePoint(*vector3.NewVector3Zero(), 50)
            if err := realtime.Send(dispatcher,
                []c.Presence{},
                &realtime.Packet_RequestSpawn{
                    RequestSpawn: &realtime.Spawn{
                        Type: &realtime.Spawn_Animal{
                            Animal: &realtime.Animal{
                                Transform: &realtime.Transform{
                                    Id:       uint64(mState.spawned),
                                    Position: &randomPos,
                                    Rotation: nil,
                                },
                            },
                        },
                    },
                }); err != nil {
                logger.Error(err.Error())
            }
            mState.spawned++
        }
	}

	return mState
}

func (m *Match) MatchTerminate(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, graceSeconds int) interface{} {
	logger.Info("MatchTerminate")
	message := "Server shutting down in " + strconv.Itoa(graceSeconds) + " seconds."
	if err := dispatcher.BroadcastMessage(2, []byte(message), nil, nil, true); err != nil {
		logger.Error(err.Error())
		return err
	}
	return state
}
