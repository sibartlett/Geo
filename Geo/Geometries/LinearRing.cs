#nullable enable
using System;
using System.Collections.Generic;

namespace Geo.Geometries;

public class LinearRing : LineString
{
    public static new readonly LinearRing Empty = new();

    public LinearRing()
        : this(new CoordinateSequence()) { }

    public LinearRing(IEnumerable<Coordinate> coordinates)
        : this(new CoordinateSequence(coordinates)) { }

    public LinearRing(params Coordinate[] coordinates)
        : this(new CoordinateSequence(coordinates)) { }

    public LinearRing(CoordinateSequence? coordinates)
        : base(coordinates)
    {
        if (coordinates != null && !coordinates.IsEmpty)
        {
            if (coordinates.Count < 4)
                throw new ArgumentException(
                    "A Linear Ring must have either zero or at least four coordinates"
                );

            if (!coordinates.IsClosed)
                throw new ArgumentException(
                    "The Coordinate Sequence must be closed to form a Linear Ring"
                );
        }
    }
}
