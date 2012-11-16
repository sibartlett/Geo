using System;

namespace Geo.Interfaces
{
    public interface ISpatialEquatable<T> : IEquatable<T>, ISpatialEquatable
    {
        bool Equals(T other, SpatialEqualityOptions options);
        bool Equals2D(T other);
        bool Equals3D(T other);
    }

    public interface ISpatialEquatable
    {
        bool Equals(object obj, SpatialEqualityOptions options);
        bool Equals2D(object obj);
        bool Equals3D(object obj);
        int GetHashCode(SpatialEqualityOptions options);
    }
}
