using System.Collections.Generic;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class Spatial3DComparer<T> : IEqualityComparer<T> where T : ISpatialEquatable<T>
    {
        public bool Equals(T x, T y)
        {
            return SpatialObject.Equals(x, y, GeoContext.Current.EqualityOptions.To2D());
        }

        public int GetHashCode(T obj)
        {
            return obj.GetHashCode(GeoContext.Current.EqualityOptions.To2D());
        }
    }
}