package volume


// Fit check if the given volume is entirely contained in the other one
func (c *Capsule) Fit(other Volume) bool {
	return false
}

// Intersects check if a volume intersects with another one
func (c *Capsule) Intersects(other Volume) bool {
	return false
}

// Average create a new volume averaged on 2 volumes
func (c *Capsule) Average(other Volume) Volume {
	return nil
}

// Mutate create a new volume with random mutations
func (c *Capsule) Mutate(rate float64) Volume {
	return nil
}
