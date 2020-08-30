package volume

import (
    "errors"
    "github.com/louis030195/protometry/api/vector3"
)

// NewBoxMinMax returns a new box using min max
func NewBoxMinMax(minX, minY, minZ, maxX, maxY, maxZ float64) *Box {
	return &Box{
		Min: vector3.NewVector3(minX, minY, minZ),
		Max: vector3.NewVector3(maxX, maxY, maxZ),
	}
}

// NewBoxOfSize returns a box of size centered at center
func NewBoxOfSize(x, y, z, size float64) *Box {
	half := size / 2
	min := *vector3.NewVector3(x-half, y-half, z-half)
	max := *vector3.NewVector3(x+half, y+half, z+half)
	return &Box{
		Min: &min,
		Max: &max,
	}
}

// Equal returns whether a box is equal to another
func (b Box) Equal(other Box) bool {
	return b.Min.Equal(*other.Min) && b.Max.Equal(*other.Max)
}

// GetCenter ...
func (b Box) GetCenter() vector3.Vector3 {
	return *b.Min.Lerp(b.Max, 0.5)
}

// GetSize returns the size of the box
func (b *Box) GetSize() vector3.Vector3 {
	return b.Max.Minus(*b.Min)
}

// In Returns whether the specified point is contained in this box.
func (b Box) Contains(v vector3.Vector3) bool {
	return (b.Min.X <= v.X && v.X <= b.Max.X) &&
		(b.Min.Y <= v.Y && v.Y <= b.Max.Y) &&
		(b.Min.Z <= v.Z && v.Z <= b.Max.Z)
}

// Fit Returns whether the specified area is fully contained in the other area.
func (b Box) Fit(o Box) bool {
	return o.Contains(*b.Max) && o.Contains(*b.Min)
}

// Intersects Returns whether any portion of this area strictly intersects with the specified area or reversely.
func (b Box) Intersects(b2 Box) bool {
	return !(b.Max.X < b2.Min.X || b2.Max.X < b.Min.X || b.Max.Y < b2.Min.Y || b2.Max.Y < b.Min.Y || b.Max.Z < b2.Min.Z || b2.Max.Z < b.Min.Z)
}

/* Split split a CUBE into 8 cubes
 *    3____7
 *  2/___6/|
 *  | 1__|_5
 *  0/___4/
 */
func (b *Box) Split() [8]*Box {
	center := b.GetCenter()
	return [8]*Box{
        NewBoxMinMax(b.Min.X, b.Min.Y, b.Min.Z, center.X, center.Y, center.Z),
        NewBoxMinMax(b.Min.X, b.Min.Y, center.Z, center.X, center.Y, b.Max.Z),
        NewBoxMinMax(b.Min.X, center.Y, b.Min.Z, center.X, b.Max.Y, center.Z),
        NewBoxMinMax(b.Min.X, center.Y, center.Z, center.X, b.Max.Y, b.Max.Z),

        NewBoxMinMax(center.X, b.Min.Y, b.Min.Z, b.Max.X, center.Y, center.Z),
        NewBoxMinMax(center.X, b.Min.Y, center.Z, b.Max.X, center.Y, b.Max.Z),
        NewBoxMinMax(center.X, center.Y, b.Min.Z, b.Max.X, b.Max.Y, center.Z),
        NewBoxMinMax(center.X, center.Y, center.Z, b.Max.X, b.Max.Y, b.Max.Z),
	}
}

/* SplitFour split a CUBE into 4 cubes
 * Vertical
 *    1____3
 *  0/___2/|
 *  | 1__|_3
 *  0/___2/
 * Horizontal
 *    1____3
 *  1/___3/|
 *  | 0__|_2
 *  0/___2/
 */
func (b *Box) SplitFour(vertical bool) [4]*Box {
    center := b.GetCenter()
    if vertical {
        return [4]*Box{
            NewBoxMinMax(b.Min.X, b.Min.Y, b.Min.Z, center.X, b.Max.Y, center.Z),
            NewBoxMinMax(b.Min.X, b.Min.Y, center.Z, center.X, b.Max.Y, b.Max.Z),
            NewBoxMinMax(center.X, b.Min.Y, b.Min.Z, b.Max.X, b.Max.Y, center.Z),
            NewBoxMinMax(center.X, b.Min.Y, center.Z, b.Max.X, b.Max.Y, b.Max.Z),
        }
    }
    return [4]*Box{
        NewBoxMinMax(b.Min.X, b.Min.Y, b.Min.Z, center.X, center.Y, b.Max.Z),
        NewBoxMinMax(b.Min.X, center.Y, b.Min.Z, center.X, b.Max.Y, b.Max.Z),
        NewBoxMinMax(center.X, b.Min.Y, b.Min.Z, b.Max.X, center.Y, b.Max.Z),
        NewBoxMinMax(center.X, center.Y, b.Min.Z, b.Max.X, b.Max.Y, b.Max.Z),
    }
}

// Grows the Bounds to include the point. In-place and returns itself
func (b *Box) EncapsulatePoint(o vector3.Vector3) *Box {
	min := vector3.Min(*b.Min, o)
	max := vector3.Max(*b.Max, o)
	b.Min = &min
	b.Max = &max
	return b
}

// Grows the Bounds to include the bounds. In-place and returns itself
func (b *Box) EncapsulateBox(o Box) *Box {
	b.EncapsulatePoint(*o.Min)
	b.EncapsulatePoint(*o.Max)
	return b
}

// Intersection returns the intersection between two boxes
func (b Box) Intersection(o Box) vector3.Vector3 {
    panic(errors.New("intersection not implemented"))
    return vector3.Vector3{}
}
