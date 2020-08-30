package octree

import (
    "fmt"
    "github.com/louis030195/protometry/api/volume"
)

// Octree ...
type Octree struct {
	root *Node
}

// NewOctree is a Octree constructor for ease of use
func NewOctree(region *volume.Box) *Octree {
	return &Octree{
		root: &Node{region: *region},
	}
}

// Insert a object in the Octree, TODO: bool or object return?
func (o *Octree) Insert(object Object) bool {
	return o.root.insert(object)
}

// Move object to a new Bounds, pass a pointer because we want to modify the passed object data
func (o *Octree) Move(object *Object, newPosition ...float64) bool {
	return o.root.move(object, newPosition...)
}

// Remove object
func (o *Octree) Remove(object Object) bool {
	return o.root.remove(object)
}

// GetColliding returns an array of objects that intersect with the specified bounds, if any.
// Otherwise returns an empty array.
func (o *Octree) GetColliding(bounds volume.Box) []Object {
	return o.root.getColliding(bounds)
}

// GetAllObjects return all objects, the returned array is sorted in the DFS order
func (o *Octree) GetAllObjects() []Object {
	return o.root.getAllObjects()
}

// Range based on https://golang.org/src/sync/map.go?s=9749:9805#L296
// Range calls f sequentially for each object present in the octree.
// If f returns false, range stops the iteration.
func (o *Octree) Range(f func(*Object) bool) {
	o.root.rang(f)
}

// Get will try to find a specific object based on an id
func (o *Octree) Get(id uint64, box volume.Box) *Object {
	objs := o.GetColliding(box)
	for _, obj := range objs {
		if id == obj.ID() {
			return &obj
		}
	}
	return nil
}

// GetSize returns the size of the Octree (cubic volume)
func (o *Octree) GetSize() int64 {
	s := o.root.region.GetSize()
    return int64(s.X)
}

// GetNodes flatten all the nodes into an array, the returned array is sorted in the DFS order
func (o *Octree) GetNodes() []Node {
	return o.root.getNodes()
}

// getHeight debug function
func (o *Octree) getHeight() int {
	return o.root.getHeight()
}

// getNumberOfNodes debug function
func (o *Octree) getNumberOfNodes() int {
	return o.root.getNumberOfNodes()
}

// getNumberOfObjects debug function
func (o *Octree) getNumberOfObjects() int {
	return o.root.getNumberOfObjects()
}

// getUsage ...
func (o *Octree) getUsage() float64 {
	return float64(o.getNumberOfObjects()) / float64(o.getNumberOfNodes()*CAPACITY)
}

func (o *Octree) toString(verbose bool) string {
	return fmt.Sprintf("Octree: {\n%v\n}", o.root.toString(verbose))
}
