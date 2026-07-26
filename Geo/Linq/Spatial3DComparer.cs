#nullable enable
using System.Collections.Generic;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;

namespace Geo.Linq;

public class Spatial3DComparer<T> : IEqualityComparer<T>
    where T : ISpatialEquatable
{
    public bool Equals(T x, T y)
    {
        return SpatialObject.Equals3D(x, y);
    }

    // One shared options instance rather than a new one per element; see
    // Spatial2DComparer.GetHashCode. The elevation is still hashed, which is what keeps
    // coordinates stacked on one position in separate buckets.
    public int GetHashCode(T obj)
    {
        return obj.GetHashCode(SpatialEqualityOptions.PositionAndElevation);
    }
}
