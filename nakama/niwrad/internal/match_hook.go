package niwrad

import (
    "context"
    "database/sql"
    "fmt"
    "github.com/louis030195/niwrad/api/realtime"
    octree "github.com/louis030195/octree/pkg"
    "github.com/louis030195/protometry/api/volume"
    "math"
    "math/rand"
    "strconv"

    "github.com/golang/protobuf/proto"
    c "github.com/heroiclabs/nakama-common/runtime"
)

var (
	tickRate     int64 = 30
	adminUserIDs []string
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
	distribution int // TODO: reduce the state as much as possible
	ready        int
	//m            *realtime.Matrix
	spawned      int
	seed         int64
}

type Match struct{}

func (m *Match) MatchInit(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, params map[string]interface{}) (interface{}, int, string) {
	//generatedMap, err := gen.DiamondSquare(int(math.Pow(2, 8)+1), 40, 1)

	//if err != nil {
	//	logger.Error(err.Error())
	//	return nil, 0, ""
	//}

	matchID := ctx.Value(c.RUNTIME_CTX_MATCH_ID).(string)
	distribution, ok := params["distribution"].(int)
	if !ok {
		logger.Error("Failed to init match, no distribution given")
		if err := stopMatch(ctx, db, matchID); err != nil {
			logger.Error(err.Error())
			return nil, 0, ""
		}
		return nil, 0, ""
	}
	for i := 0; i < distribution; i++ {
		userID, ok := params[fmt.Sprintf("admin%d", i)].(string)
		if !ok {
			logger.Error("Failed to init match, couldn't retrieve admin user ID")
			if err := stopMatch(ctx, db, matchID); err != nil {
				logger.Error(err.Error())
				return nil, 0, ""
			}
			return nil, 0, ""
		}
		adminUserIDs = append(adminUserIDs, userID)
	}
	if !ok {
		logger.Error("Failed to init match, no admins")
		if err := stopMatch(ctx, db, matchID); err != nil {
			logger.Error(err.Error())
			return nil, 0, ""
		}
		return nil, 0, ""
	}

	size := 1000.
	region := volume.NewBoxOfSize(0, 0, 0, size)
	oc := octree.NewOctree(region)
	seed := rand.Int63()
	rand.Seed(seed)
	state := &MatchState{
		matchID:      matchID,
		presences:    make(map[string]PresenceState),
		octree:       oc,
		distribution: distribution,
		//m:            generatedMap,
		seed:         seed,
	}
	logger.Info("MatchInit, params: %v, state: %v", params, state)
	return state, int(tickRate), ""
}

func (m *Match) MatchJoinAttempt(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presence c.Presence, metadata map[string]string) (interface{}, bool, string) {
	logger.Info("MatchJoinAttempt, %v", presence)
	return state, true, ""
}

func (m *Match) MatchJoin(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchJoin, %v", presences)
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		// Initial client spawn, TODO: config something
		b := volume.NewBoxOfSize(5, 0, 5, 500)
		for i := range adminUserIDs {
			if p.GetUserId() == adminUserIDs[i] {
			    var bs []*volume.Box
			    oc := volume.NewBoxOfSize(0, 0, 0, float64(mState.octree.GetSize()))
                switch mState.distribution {
                case 1:
                    bs = append(bs, oc)
                case 2:
                    panic("Not implemented")
                case 4:
                    // TODO Split(n)
                    for _, sb := range oc.SplitFour(true) {
                        bs = append(bs, sb)
                    }
                case 8:
                    for _, sb := range oc.Split() {
                        bs = append(bs, sb)
                    }
                }
				b = bs[len(mState.servers)]
				mState.servers = append(mState.servers, b)
				logger.Info("New match server, region: %v ", b)
				if len(mState.servers) == mState.distribution {
					logger.Info("All executors joined (%d)", mState.distribution)
				}
				break
			}
		}
		mState.presences[p.GetUserId()] = PresenceState{presence: p, region: b}
		logger.Info("Sending presence seed and region: %v, %v", mState.seed, b)
		if err := realtime.Send(dispatcher, []c.Presence{p}, &realtime.Packet_MatchJoin{
			MatchJoin: &realtime.MatchJoin{
				Region: b,
				Seed:   mState.seed,
			},
		}); err != nil {
			logger.Error(err.Error())
			return nil
		}
	}

	// Just a last check, shouldn't be too expensive
	//for i := 0; i < len(mState.servers)-1; i++ {
	//    if mState.servers[i].Intersects(*mState.servers[i+1]) {
	//        logger.Error("Servers region shouldn't overlap !")
	//        return nil
	//    }
	//}
	return state
}

func (m *Match) MatchLeave(ctx context.Context, logger c.Logger, db *sql.DB, nk c.NakamaModule, dispatcher c.MatchDispatcher, tick int64, state interface{}, presences []c.Presence) interface{} {
	logger.Info("MatchLeave")
	mState, _ := state.(*MatchState)
	for _, p := range presences {
		logger.Info("UserID: %v", p.GetUserId())
		delete(mState.presences, p.GetUserId())
	}

	if len(mState.presences) == 0 {
		logger.Info("Match %v is empty, terminating it", mState.matchID)
		//m.MatchTerminate(ctx, logger, db, nk, dispatcher, tick, state, 2)
		return nil
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
        logger.Info("Received %T, impact %v, full: %v", s.Type, s.Impact, s)
		logger.Info("ID:%v;SESSION:%v;USERNAME:%v", message.GetUserId(), message.GetSessionId(), message.GetUsername())

		// TODO: let's reduce as much as possible computing on nakama side which isn't distributed
		// TODO: and thus is likely to be not that much scalable
		switch x := s.Type.(type) {
        case *realtime.Packet_RequestSpawn:
            tmp, _ := x.RequestSpawn.Type.(*realtime.Spawn_Animal)
            logger.Info("req spawn at %v", tmp.Animal.Transform.Position)
        case *realtime.Packet_Initialized:
			if mState.ready < mState.distribution {
			    mState.ready++
			    logger.Info("Server %s, is ready, match readiness %d/%d", message.GetUserId(), mState.ready, mState.distribution)
			    if mState.ready == mState.distribution {
                    if err := dispatcher.MatchLabelUpdate(ReadyLabel); err != nil {
                        logger.Error(err.Error())
                        return nil
                    }
                    logger.Info("%d/%d executors joined and are ready, clients can join the match now",
                        mState.ready, mState.distribution)
                }
            }
		}

		// TODO: optimize as much as possible here, main loop
		// nakama server will only send to recipients concerned (in the region of impact)
		var presences []c.Presence
        for _, v := range mState.presences {
            // If the presence should be impacted
            // nil impact = global
            if v.presence.GetUserId() != message.GetUserId() &&
                (s.Impact == nil || math.IsInf(s.Impact.X, 1) || v.region.Contains(*s.Impact)) {
                presences = append(presences, v.presence)
            }
        }

        if len(presences) > 0 {
            logger.Info("Sending message %T from %s to %v", s.Type, message.GetUserId(), presences)
            err := dispatcher.BroadcastMessage(1, message.GetData(), presences, nil, true)
            if err != nil {
                logger.Error(err.Error())
                return err
            }
        }
	}

	// Stop match if empty after a while
	//if tick > tickRate*3 && len(mState.presences) == 0 {
	//	logger.Info("Match %v is empty, terminating it", mState.matchID)
	//	// Terminate match when empty
	//	m.MatchTerminate(ctx, logger, db, nk, dispatcher, tick, state, 2)
	//	return nil
	//}

	//if len(mState.servers) == mState.distribution { // When all servers joined
	//	for i := 0; i < 5; i++ {
	//		// I suppose server handling that request will do the work to adjust above ground by ray-casting
	//		// avoid like the plague physics here :)
	//		randomPos := vector3.RandomSpherePoint(*vector3.NewVector3Zero(), 50)
	//		if err := realtime.Send(dispatcher,
	//			[]c.Presence{},
	//			&realtime.Packet_RequestSpawn{
	//				RequestSpawn: &realtime.Spawn{
	//					Type: &realtime.Spawn_Animal{
	//						Animal: &realtime.Animal{
	//							Transform: &realtime.Transform{
	//								Id:       uint64(mState.spawned),
	//								Position: &randomPos,
	//								Rotation: nil,
	//							},
	//						},
	//					},
	//				},
	//			}); err != nil {
	//			logger.Error(err.Error())
	//		}
	//		mState.spawned++
	//	}
	//}

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
