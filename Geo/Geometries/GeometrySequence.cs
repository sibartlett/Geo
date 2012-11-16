using System.Collections.Generic;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class GeometrySequence<T> : SpatialReadOnlyCollection<GeometrySequence<T>, T> where T : IGeometry, ISpatialEquatable<T>
    {
        public GeometrySequence() : base(new List<T>())
        {
        }

        public GeometrySequence(IEnumerable<T> geometries) : base(geometries.ToList())
        {
        }

        public GeometrySequence(params T[] geometries) : base(geometries.ToList())
        {
        }

        #region Equality methods

        public override bool Equals(GeometrySequence<T> other, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (Count != other.Count)
                return false;

            return !this
                .Where((t, i) => !t.Equals(other[i], options))
                .Any();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(GeometrySequence<T> left, GeometrySequence<T> right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(GeometrySequence<T> left, GeometrySequence<T> right)
        {
            return !(left == right);
        }

        #endregion
    }
}
