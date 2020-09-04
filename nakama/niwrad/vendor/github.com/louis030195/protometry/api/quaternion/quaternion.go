package quaternion

import (
	"math"
)

// NewQuaternion constructs a Vector3
func NewQuaternion(x, y, z, w float64) *Quaternion {
	return &Quaternion{X: x, Y: y, Z: z, W: w}
}

// ToQuaternion ... yaw (Z), pitch (Y), roll (X)
func ToQuaternion(yaw, pitch, roll float64) *Quaternion {
	// Abbreviations for the various angular functions
	cy := math.Cos(yaw * 0.5)
	sy := math.Sin(yaw * 0.5)
	cp := math.Cos(pitch * 0.5)
	sp := math.Sin(pitch * 0.5)
	cr := math.Cos(roll * 0.5)
	sr := math.Sin(roll * 0.5)

	return NewQuaternion(cy*cp*cr+sy*sp*sr, cy*cp*sr-sy*sp*cr, sy*cp*sr+cy*sp*cr, sy*cp*cr-cy*sp*sr)
}
