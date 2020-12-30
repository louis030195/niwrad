package vector3

import (
    "github.com/louis030195/protometry/api/quaternion"
    "math"
    "math/rand"
)

// NewVector3 constructs a Vector3
func NewVector3(x, y, z float64) *Vector3 {
	return &Vector3{X: x, Y: y, Z: z}
}

// Clone a vector
func (v *Vector3) Clone() *Vector3 {
	return NewVector3(v.X, v.Y, v.Z)
}

// NewVector3Zero constructs a Vector3 of 3 dimensions initialized with 0
func NewVector3Zero() *Vector3 {
	return NewVector3(0, 0, 0)
}

// NewVector3One constructs a Vector3 of 3 dimensions initialized with 1
func NewVector3One() *Vector3 {
	return NewVector3(1, 1, 1)
}

// NewVector3Max returns a Vector3 of maximum float64 value
func NewVector3Max() *Vector3 {
	return NewVector3(math.MaxFloat64, math.MaxFloat64, math.MaxFloat64)
}

// NewVector3Min returns a Vector3 of minimum float64 value
func NewVector3Min() *Vector3 {
	return NewVector3(-math.MaxFloat64, -math.MaxFloat64, -math.MaxFloat64)
}

// Equal reports whether a and b are equal within a small epsilon.
func (v Vector3) Equal(v2 Vector3) bool {
	const epsilon = 1e-16

	// If any dimensions aren't approximately equal, return false
	if math.Abs(v.X-v2.X) >= epsilon ||
		math.Abs(v.Y-v2.Y) >= epsilon ||
		math.Abs(v.Z-v2.Z) >= epsilon {
		return false
	}

	// Else return true
	return true
}

// Pow returns the vector pow as a new vector
// Not in-place
func (v Vector3) Pow() Vector3 {
	v.X *= v.X
	v.Y *= v.Y
	v.Z *= v.Z
	return v
}

// Sum returns the sum of all the dimensions of the vector
func (v Vector3) Sum() float64 {
	return v.X + v.Y + v.Z
}

// Norm returns the norm.
func (v Vector3) Norm() float64 { return v.Pow().Sum() }

// Norm2 returns the square of the norm.
func (v Vector3) Norm2() float64 { return math.Sqrt(v.Norm()) }

// Normalize returns a new unit vector in the same direction as a.
func (v Vector3) Normalize() Vector3 {
	n2 := v.Norm2()
	if n2 == 0 {
		return *NewVector3(0, 0, 0)
	}
	return v.Times(1 / math.Sqrt(n2))
}

// Abs returns the vector with non-negative components.
func (v *Vector3) Abs() Vector3 {
	nv := NewVector3(math.Abs(v.X), math.Abs(v.Y), math.Abs(v.Z))
	return *nv
}

// Plus returns the standard vector sum of v1 and v2.
// Not in-place
func (v Vector3) Plus(v2 Vector3) Vector3 {
	v.X += v2.X
	v.Y += v2.Y
	v.Z += v2.Z
	return v
}

// Add arguments element-wise
// v, v2 : Vector3
// The arrays to be added.
// In-place
func (v *Vector3) Add(v2 *Vector3) {
	v.X += v2.X
	v.Y += v2.Y
	v.Z += v2.Z
}

// Minus returns the standard vector difference of a and b as a new vector
// Not in-place
func (v Vector3) Minus(v2 Vector3) Vector3 {
	v.X -= v2.X
	v.Y -= v2.Y
	v.Z -= v2.Z
	return v
}

// Subtract arguments element-wise in-place
// v, v2 : Vector3
// The arrays to be subtracted.
func (v *Vector3) Subtract(v2 *Vector3) {
	v.X -= v2.X
	v.Y -= v2.Y
	v.Z -= v2.Z
}

// Times returns the scalar product of v and m
// Not in-place
func (v Vector3) Times(m float64) Vector3 {
	v.X *= m
	v.Y *= m
	v.Z *= m
	return v
}

// Scale rescale the vector3 by m
// In-place
func (v *Vector3) Scale(m float64) {
	v.X *= m
	v.Y *= m
	v.Z *= m
}

// Divide will obviously panic in case of division by 0
// In-place
func (v *Vector3) Divide(m float64) {
	v.X /= m
	v.Y /= m
	v.Z /= m
}

// Dot returns the standard dot product of a and b.
func (v Vector3) Dot(v2 Vector3) float64 {
	return (v.X * v2.X) + (v.Y * v2.Y) + (v.Z * v2.Z)
}

// Cross returns the standard cross product of a and b.
func (v Vector3) Cross(v2 Vector3) *Vector3 {
	return NewVector3(v.Y*v2.Z-v.Z*v2.Y, v.Z*v2.X-v.X*v2.Z, v.X*v2.Y-v.Y*v.X)
}

// Distance returns the Euclidean distance between a and b.
func (v Vector3) Distance(v2 Vector3) float64 { return math.Sqrt(v.Minus(v2).Pow().Sum()) }

// Angle returns the angle between a and b.
func (v Vector3) Angle(v2 Vector3) float64 {
	return math.Atan2(v.Cross(v2).Norm(), v.Dot(v2))
}

// Min Returns the a vector where each component is the lesser of the
// corresponding component in this and the specified vector.
// Not in-place
func Min(v Vector3, v2 Vector3) Vector3 {
	return *NewVector3(math.Min(v.X, v2.X), math.Min(v.Y, v2.Y), math.Min(v.Z, v2.Z))
}

// Max Returns the a vector where each component is the greater of the
// corresponding component in this and the specified vector.
// Not in-place
func Max(v Vector3, v2 Vector3) Vector3 {
	return *NewVector3(math.Max(v.X, v2.X), math.Max(v.Y, v2.Y), math.Max(v.Z, v2.Z))
}

// Lerp Returns the linear interpolation between two Vector3(s).
// Not in-place
func (v *Vector3) Lerp(v2 *Vector3, f float64) *Vector3 {
	return NewVector3((v2.X-v.X)*f+v.X, (v2.Y-v.Y)*f+v.Y, (v2.Z-v.Z)*f+v.Z)
}

// Expands a 10-bit integer into 30 bits
// by inserting 2 zeros after each bit.
func expandBits(v uint) uint {
	v = (v * 0x00010001) & 0xFF0000FF
	v = (v * 0x00000101) & 0x0F00F00F
	v = (v * 0x00000011) & 0xC30C30C3
	v = (v * 0x00000005) & 0x49249249
	return v
}

// Morton3D Calculates a 30-bit Morton code for the
// given 3D point located within the unit cube [0,1].
func Morton3D(v Vector3) uint { // TODO: decoder
	x := math.Min(math.Max(v.X*1024.0, 0.0), 1023.0)
	y := math.Min(math.Max(v.Y*1024.0, 0.0), 1023.0)
	z := math.Min(math.Max(v.Z*1024.0, 0.0), 1023.0)
	xx := expandBits(uint(x))
	yy := expandBits(uint(y))
	zz := expandBits(uint(z))
	return xx*4 + yy*2 + zz
}

func randFloat(min, max float64) float64 {
	return min + rand.Float64()*(max-min)
}

// RandomCirclePoint returns a random circle point
func RandomCirclePoint(x, z, radius float64) Vector3 {
	return *NewVector3(randFloat(-radius+x, radius+x),
		0,
		randFloat(-radius+z, radius+z))
}

// RandomSpherePoint returns a random sphere point
func RandomSpherePoint(center Vector3, radius float64) Vector3 {
	return *NewVector3(randFloat(-radius+center.X, radius+center.X),
		randFloat(-radius+center.Y, radius+center.Y),
		randFloat(-radius+center.Z, radius+center.Z))
}

// LookAt return a quaternion corresponding to the rotation required to look at the other Vector3
func (v Vector3) LookAt(b Vector3) quaternion.Quaternion {
	angle := v.Angle(b)
	return *quaternion.NewQuaternion(0, angle, 0, angle)
}

// Mutate returns a new Vector3 with each coordinates multiplied by a random value between -rate and rate
func (v Vector3) Mutate(rate float64) Vector3 {
	return *NewVector3(v.X*randFloat(-rate, rate), v.Y*randFloat(-rate, rate), v.Z*randFloat(-rate, rate))
}
