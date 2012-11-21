using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Geo.Abstractions.Interfaces;

namespace Geo.Abstractions
{
    public class SpatialReadOnlyCollection<TElement> : ReadOnlyCollection<TElement>, ISpatialEquatable
        where TElement : ISpatialEquatable
    {
        public SpatialReadOnlyCollection(IEnumerable<TElement> list) : base(list.ToList())
        {
        }

        public bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as SpatialReadOnlyCollection<TElement>;

            if (ReferenceEquals(null, other))
                return false;

            if (Count != other.Count)
                return false;

            return !this
                .Where((t, i) => !SpatialObject.Equals(t, other[i], options))
                .Any();
        }

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
    }
}