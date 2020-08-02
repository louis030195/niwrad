package realtime

import (
	"github.com/golang/protobuf/proto"
	"github.com/heroiclabs/nakama-common/runtime"
    "github.com/louis030195/protometry/api/volume"
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
	if err := dispatcher.BroadcastMessage(1, msgBytes, presences, nil, true); err != nil {
		return err
	}
	return nil
}

func (m Matrix) Send(dispatcher runtime.MatchDispatcher, presences []runtime.Presence, region volume.Box, batchSize int) error {
    chunk := Matrix{Rows: []*Array{}}
    for i := 0; i < batchSize; i++ {
        chunk.Rows = append(chunk.Rows, &Array{Cols: make([]float64, batchSize)})
    }
    for i := int(region.Min.X); i < int(region.Max.X); i+=batchSize {
        for j := int(region.Min.Z); j < int(region.Max.Z); j+=batchSize {
            m.Take(&chunk, i, j, batchSize)
            if err := Send(dispatcher, presences, &Packet_Map{
                Map: nil,
            }); err != nil {
                return err
            }
        }
    }
    return nil
}

// Take is a C# like batch iterator, allocation free for Matrix
func (m Matrix) Take(chunk *Matrix, offsetX, offsetY, batchSize int) {
    // Sending chunk of map
    for i := 0; i < batchSize && offsetX+i < len(m.Rows[0].Cols); i++ {
        for j := 0; j < batchSize && offsetY+j < len(m.Rows); j++ {
            chunk.Rows[j].Cols[i] = m.Rows[offsetY+j].Cols[offsetX+i]
        }
    }
}

