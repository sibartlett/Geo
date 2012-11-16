using Geo.Interfaces;

namespace Geo.Geometries
{
    public abstract class SpatialObject : ISpatialEquatable
    {
        public abstract bool Equals(object obj, SpatialEqualityOptions options);
        public abstract int GetHashCode(SpatialEqualityOptions options);

        public override int GetHashCode()
        {
            return GetHashCode(GeoContext.Current.EqualityOptions);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, GeoContext.Current.EqualityOptions);
        }

        public bool Equals2D(object obj)
        {
            return Equals(obj, GeoContext.Current.EqualityOptions.To2D());
        }

        public bool Equals3D(object obj)
        {
            return Equals(obj, GeoContext.Current.EqualityOptions.To3D());
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

    public abstract class SpatialObject<TSelf> : SpatialObject, ISpatialEquatable<TSelf>
    {
        public abstract bool Equals(TSelf other, SpatialEqualityOptions options);

        public bool Equals(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions);
        }

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TSelf) obj, options);
        }

        public bool Equals2D(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions.To2D());
        }

        public bool Equals3D(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions.To3D());
        }

        public static bool Equals<T>(T obj1, T obj2, SpatialEqualityOptions options) where T : ISpatialEquatable<T>
        {
            if (!ReferenceEquals(null, obj1))
                return obj1.Equals(obj2, options);

            return ReferenceEquals(null, obj2);
        }
    }
}
