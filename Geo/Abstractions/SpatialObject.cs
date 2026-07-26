#nullable enable
using Geo.Abstractions.Interfaces;

namespace Geo.Abstractions;

public abstract class SpatialObject : ISpatialEquatable
{
    public abstract bool Equals(object? obj, SpatialEqualityOptions options);

    public abstract int GetHashCode(SpatialEqualityOptions options);

    public bool Equals2D(object? obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions.To2D());
    }

    public bool Equals3D(object? obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions.To3D());
    }

    /// <remarks>
    /// Hashed under fixed options rather than whichever are in force, because a hash has to
    /// hold still for as long as the object is a key. Reading
    /// <see cref="GeoContext.Current" /> here meant a dictionary or a set could stop finding
    /// an entry it already held the moment anything changed the ambient options - and,
    /// while both settings were live, could report an item absent that its own
    /// <see cref="Equals(object)" /> called equal. Only <see cref="Equals(object)" /> now
    /// answers to the ambient options; the hash it has to agree with is the coarser one
    /// that is correct under all of them.
    /// </remarks>
    public override int GetHashCode()
    {
        return GetHashCode(SpatialEqualityOptions.PositionOnly);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions);
    }

    public static bool Equals(object? obj1, object? obj2, SpatialEqualityOptions options)
    {
        var spatialObj = obj1 as ISpatialEquatable;
        if (!ReferenceEquals(null, spatialObj))
            return spatialObj.Equals(obj2, options);

        return Equals(obj1, obj2);
    }

    public static bool Equals2D(object? obj1, object? obj2)
    {
        return Equals(obj1, obj2, GeoContext.Current.EqualityOptions.To2D());
    }

    public static bool Equals3D(object? obj1, object? obj2)
    {
        return Equals(obj1, obj2, GeoContext.Current.EqualityOptions.To3D());
    }
}
