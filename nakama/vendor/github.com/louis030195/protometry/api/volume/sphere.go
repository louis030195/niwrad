package volume

// Fit check if the given volume is entirely contained in the other one
func (s *Sphere) Fit(other Volume) bool {
	return false
}

// Intersects check if a volume intersects with another one
func (s *Sphere) Intersects(other Volume) bool {
	return false
}

// Average create a new volume averaged on 2 volumes
func (s *Sphere) Average(other Volume) Volume {
	return nil
}

// Mutate create a new volume with random mutations
func (s *Sphere) Mutate(rate float64) Volume {
	return nil
}
