using System.Collections.Generic;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class Spatial2DComparer<T> : IEqualityComparer<T> where T : ISpatialEquatable<T>
    {
        public bool Equals(T x, T y)
        {
            return SpatialObject.Equals2D(x, y);
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode();
        }
    }
}