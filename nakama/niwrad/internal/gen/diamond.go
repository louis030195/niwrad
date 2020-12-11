package gen
//
//import (
//	"errors"
//	"github.com/louis030195/niwrad/api/realtime"
//	"math/rand"
//)
//
//// TODO: make the code less ugly as hell
//// DiamondSquare returns a terrain procedurally generated using Diamond Square algorithm
//////size of grid to generate, note this must be a
//////value 2^n+1
//func DiamondSquare(terrainPoints int, roughness float64, seed float64) (*realtime.Matrix, error) {
//	if terrainPoints%2 != 1 {
//		return nil, errors.New("must be a power of two plus one")
//	}
//	dataSize := terrainPoints // must be a power of two plus one
//	data := realtime.Matrix{Rows: []*realtime.Array{}}
//	for i := 0; i < dataSize; i++ {
//		data.Rows = append(data.Rows, &realtime.Array{Cols: make([]float64, dataSize)})
//	}
//	rand.Seed(int64(seed))
//	data.Rows[0].Cols[0], data.Rows[0].Cols[dataSize-1], data.Rows[dataSize-1].Cols[0], data.Rows[dataSize-1].Cols[dataSize-1] = seed, seed, seed, seed
//	h := roughness //the range (-h -> +h) for the average offset - affects roughness
//
//	for sideLength := dataSize - 1; sideLength >= 2; {
//
//		halfSide := sideLength / 2
//
//		//generate the new square values
//		for x := 0; x < dataSize-1; x += sideLength {
//			for y := 0; y < dataSize-1; y += sideLength {
//				//x, y is upper left corner of square
//				//calculate average of existing corners
//				avg := data.Rows[x].Cols[y] + data.Rows[x+sideLength].Cols[y] +
//					data.Rows[x].Cols[y+sideLength] + data.Rows[x+sideLength].Cols[y+sideLength]
//				avg /= 4.0
//
//				//center is average plus random offset
//				data.Rows[x+halfSide].Cols[y+halfSide] = avg + (rand.Float64() * 2 * h) - h
//			}
//		}
//		//generate the diamond values
//		//since the diamonds are staggered we only move x
//		//by half side
//		//NOTE: if the data shouldn't wrap then x < DATA_SIZE
//		//to generate the far edge values
//		for x := 0; x < dataSize-1; x += halfSide {
//			//and y is x offset by half a side, but moved by
//			//the full side length
//			//NOTE: if the data shouldn't wrap then y < DATA_SIZE
//			//to generate the far edge values
//			for y := (x + halfSide) % sideLength; y < dataSize-1; y += sideLength {
//				//x, y is center of diamond
//				//note we must use mod  and add DATA_SIZE for subtraction
//				//so that we can wrap around the array to find the corners
//				avg := data.Rows[(x-halfSide+dataSize)%dataSize].Cols[y] +
//					data.Rows[(x+halfSide)%dataSize].Cols[y] +
//					data.Rows[x].Cols[(y+halfSide)%dataSize] + //below center
//					data.Rows[x].Cols[(y-halfSide+dataSize)%dataSize] //above center
//				avg /= 4.0
//
//				//new value = average plus random offset
//				//We calculate random value in range of 2h
//				//and then subtract h so the end value is
//				//in the range (-h, +h)
//				avg = avg + (rand.Float64() * 2 * h) - h
//				//update value for center of diamond
//				data.Rows[x].Cols[y] = avg
//
//				//wrap values on the edges, remove
//				//this and adjust loop condition above
//				//for non-wrapping values.
//				if x == 0 {
//					data.Rows[dataSize-1].Cols[y] = avg
//				}
//				if y == 0 {
//					data.Rows[x].Cols[dataSize-1] = avg
//				}
//			}
//		}
//		sideLength /= 2
//		h /= 2.0
//	}
//	return &data, nil
//}
