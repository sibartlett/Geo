using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public abstract class SpatialReadOnlyCollection<TSelf, TElement> : ReadOnlyCollection<TElement>, ISpatialEquatable<TSelf> where TElement : ISpatialEquatable<TElement>
    {
        protected SpatialReadOnlyCollection(IList<TElement> list) : base(list)
        {
        }

        public abstract bool Equals(TSelf other, SpatialEqualityOptions options);

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public int GetHashCode(SpatialEqualityOptions options)
        {
            return this
                .Select(x => x.GetHashCode(options))
                .Aggregate(0, (current, result) => (current * 397) ^ result);
        }

        public bool Equals(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions);
        }

        public bool Equals(object obj, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((TSelf) obj, options);
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

        public bool Equals2D(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions.To2D());
        }

        public bool Equals3D(TSelf other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions.To3D());
        }

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