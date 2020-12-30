package volume

// Volume is a 3-d interface representing volumes like Boxes, Spheres, Capsules ...
type Volume interface {
	// Fit check if the given volume is entirely contained in the other one
	Fit(Volume) bool
	// Intersects check if a volume intersects with another one
	Intersects(Volume) bool
	// Average create a new volume averaged on 2 volumes
	Average(Volume) Volume
	// Mutate create a new volume with random mutations
	Mutate(float64) Volume
}
