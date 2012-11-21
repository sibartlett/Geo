using System;
using System.Collections.Generic;

namespace Geo.Geometries
{
    public class LinearRing : LineString
    {
        public new static readonly LinearRing Empty = new LinearRing();

        public LinearRing() : this(new CoordinateSequence())
        {
        }

        public LinearRing(IEnumerable<Coordinate> coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LinearRing(params Coordinate[] coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LinearRing(CoordinateSequence coordinates) : base(coordinates)
        {
            if (coordinates != null && !coordinates.IsEmpty && !coordinates.IsClosed)
                throw new ArgumentException("The Coordinate Sequence must be closed to form a Linear Ring");
        }
    }
}
