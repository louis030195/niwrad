package realtime

import (
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/runtime"
)

// Send is a helper for nakama server -> client packet sending
func Send(dispatcher runtime.MatchDispatcher, presences []runtime.Presence, p isPacket_Type) error {
	msg := Packet{
		SenderId:   "",
		IsServer:   false,
		Recipients: nil,
		Impact:     nil,
		Type:       p,
	}
	msgBytes, err := proto.Marshal(&msg)
	if err != nil {
		return err
	}
	if err := dispatcher.BroadcastMessageDeferred(0, msgBytes, presences, nil, true); err != nil {
		return err
	}
	return nil
}
