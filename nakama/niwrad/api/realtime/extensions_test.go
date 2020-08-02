package realtime

import (
    "github.com/louis030195/protometry/api/volume"
    "math/rand"
    "testing"
)

func (m *Matrix) print(t *testing.T) {
    for i := 0; i < len(m.Rows); i ++ {
        for j := 0; j < len(m.Rows[0].Cols); j ++ {
            t.Logf("[%d;%d]:%0.1f", i, j, m.Rows[i].Cols[j])
        }
    }
}

func Test_chunkOfMap(t *testing.T) {
	data := Matrix{Rows: []*Array{
		{
			Cols: []float64{1, 2, 3, 4, 5, 6, 7, 8, 9, 10},
		},
        {
            Cols: []float64{11, 12, 13, 14, 15, 16, 17, 18, 19, 20},
        },
	}}
	chunk := Matrix{Rows: []*Array{}}
	batch := 2
	for i := 0; i < batch; i++ {
		chunk.Rows = append(chunk.Rows, &Array{Cols: make([]float64, batch)})
	}
    data.Take(&chunk, 0, 0, batch)
    if chunk.Rows[0].Cols[0] != 1 && chunk.Rows[0].Cols[1] != 2 &&
        chunk.Rows[1].Cols[0] != 11 && chunk.Rows[1].Cols[1] != 12 {
        t.FailNow()
	}

    chunk = Matrix{Rows: []*Array{}}
    batch = 5
    for i := 0; i < batch; i++ {
        chunk.Rows = append(chunk.Rows, &Array{Cols: make([]float64, batch)})
    }
    data.Take(&chunk, 1, 1, batch)
    chunk.print(t)
    if chunk.Rows[0].Cols[3] != 15 {
        t.FailNow()
    }
}

func Test_chunkOfMapFromRegion(t *testing.T) {
    dataSize := 1000
    data := Matrix{Rows: []*Array{}}
    for i := 0; i < dataSize; i++ {
        data.Rows = append(data.Rows, &Array{Cols: make([]float64, dataSize)})
        for j := 0; j < len(data.Rows[0].Cols); j++ {
            data.Rows[i].Cols[j] = float64(i)
        }
    }
    batchSize := 10
    region := volume.NewBoxOfSize(100, 100, 100, 200)
    chunk := Matrix{Rows: []*Array{}}
    for i := 0; i < batchSize; i++ {
        chunk.Rows = append(chunk.Rows, &Array{Cols: make([]float64, batchSize)})
    }
    for i := int(region.Min.X); i < int(region.Max.X); i+=batchSize {
        for j := int(region.Min.Z); j < int(region.Max.Z); j+=batchSize {
            m.Take(&chunk, i, j, batchSize)
        }
    }
}
