namespace Geo.Abstractions.Interfaces;

public interface ISpatialEquatable
{
    bool Equals(object obj, SpatialEqualityOptions options);
    bool Equals2D(object obj);
    bool Equals3D(object obj);
    int GetHashCode(SpatialEqualityOptions options);
}