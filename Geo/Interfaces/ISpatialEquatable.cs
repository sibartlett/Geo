using System;

namespace Geo.Interfaces
{
    public interface ISpatialEquatable<T> : IEquatable<T>, ISpatialEquatable
    {
        bool Equals(T other, SpatialEqualityOptions options);
    }

    public interface ISpatialEquatable
    {
        bool Equals(object other, SpatialEqualityOptions options);
    }
}
