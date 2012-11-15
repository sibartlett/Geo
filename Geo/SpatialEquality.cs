using Geo.Interfaces;

namespace Geo
{
    public static class SpatialEquality
    {
        public static bool Equals(object obj1, object obj2, SpatialEqualityOptions options)
        {
            var spatialObj = obj1 as ISpatialEquatable;
            if (!ReferenceEquals(null, spatialObj))
                return spatialObj.Equals(obj2, options);

            return Equals(obj1, obj2);
        }

        public static bool Equals<T>(T obj1, T obj2, SpatialEqualityOptions options) where T : ISpatialEquatable<T>
        {
            if (!ReferenceEquals(null, obj1))
                return obj1.Equals(obj2, options);

            return ReferenceEquals(null, obj2);
        }
    }
}