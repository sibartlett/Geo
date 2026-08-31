#nullable enable
using System.Collections.Generic;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;

namespace Geo.Linq;

public class Spatial2DComparer<T> : IEqualityComparer<T>
    where T : ISpatialEquatable
{
    public bool Equals(T? x, T? y)
    {
        return SpatialObject.Equals2D(x, y);
    }

    // To2D() builds a fresh options object, and this runs once per element - a Distinct2D
    // over a hundred thousand coordinates allocated a hundred thousand of them. The two
    // settings it carried over from the ambient options, PoleCoordiantesAreEqual and
    // AntiMeridianCoordinatesAreEqual, no longer reach a hash, so one shared instance
    // gives the same answer as a new one every time.
    public int GetHashCode(T obj)
    {
        return obj.GetHashCode(SpatialEqualityOptions.PositionOnly);
    }
}
