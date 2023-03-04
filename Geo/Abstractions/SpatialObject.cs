using Geo.Abstractions.Interfaces;

namespace Geo.Abstractions;

public abstract class SpatialObject : ISpatialEquatable
{
    public abstract bool Equals(object obj, SpatialEqualityOptions options);

    public abstract int GetHashCode(SpatialEqualityOptions options);

    public bool Equals2D(object obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions.To2D());
    }

    public bool Equals3D(object obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions.To3D());
    }

    public override int GetHashCode()
    {
        return GetHashCode(GeoContext.Current.EqualityOptions);
    }

    public override bool Equals(object obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions);
    }

    public static bool Equals(object obj1, object obj2, SpatialEqualityOptions options)
    {
        var spatialObj = obj1 as ISpatialEquatable;
        if (!ReferenceEquals(null, spatialObj))
            return spatialObj.Equals(obj2, options);

        return Equals(obj1, obj2);
    }

    public static bool Equals2D(object obj1, object obj2)
    {
        return Equals(obj1, obj2, GeoContext.Current.EqualityOptions.To2D());
    }

    public static bool Equals3D(object obj1, object obj2)
    {
        return Equals(obj1, obj2, GeoContext.Current.EqualityOptions.To3D());
    }
}