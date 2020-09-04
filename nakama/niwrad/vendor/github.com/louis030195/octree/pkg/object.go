package octree

import (
    "fmt"
    "github.com/louis030195/protometry/api/volume"

    "sync/atomic"
)

var (
	idInc uint64
)

// Identifier is an interface for anything that implements the basic ID() uint64,
// as the object does.  It is useful as more specific interface for an
// object registry than just the interface{} interface
type Identifier interface {
	ID() uint64
}

// IdentifierSlice implements the sort.Interface, so you can use the
// store objects in slices, and use the P=n*log n lookup for them
type IdentifierSlice []Identifier

func newID() uint64 {
	return atomic.AddUint64(&idInc, 1)
}

// Len returns the length of the underlying slice
// part of the sort.Interface
func (is IdentifierSlice) Len() int {
	return len(is)
}

// Less will return true if the ID of element at i is less than j;
// part of the sort.Interface
func (is IdentifierSlice) Less(i, j int) bool {
	return is[i].ID() < is[j].ID()
}

// Swap the elements at positions i and j
// part of the sort.Interface
func (is IdentifierSlice) Swap(i, j int) {
	is[i], is[j] = is[j], is[i]
}

// Object stores data and bounds
type Object struct {
	id     uint64
	Data   interface{}
	Bounds volume.Box
}

// NewObject is a Object constructor with bounds for ease of use
func NewObject(data interface{}, bounds volume.Box) *Object {
	return &Object{id: newID(), Data: data, Bounds: bounds}
}

// NewObjectCube returns a new cubic object of given size
func NewObjectCube(data interface{}, x, y, z, size float64) *Object {
	return NewObject(data, *volume.NewBoxOfSize(x, y, z, size))
}

// ID returns the unique identifier of the entity.
func (o *Object) ID() uint64 {
	return o.id
}

// Equal checks object equality over an atomic ID property
func (o *Object) Equal(object Object) bool {
	return o.id == object.id
}

func (o *Object) String() string {
	return fmt.Sprintf("Data:%v\nBounds:{\n%v\n}", o.Data, o.Bounds)
}
