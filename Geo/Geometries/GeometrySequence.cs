using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class GeometrySequence<T> : ReadOnlyCollection<T> where T : IGeometry
    {
        public GeometrySequence() : base(new List<T>())
        {
        }

        public GeometrySequence(IEnumerable<T> geometries) :base(geometries.ToList())
        {
        }

        public GeometrySequence(params T[] geometries) : base(geometries.ToList())
        {
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        #region Equality methods

        protected bool Equals(GeometrySequence<T> other)
        {
            if (other == null)
                return false;

            if (Count != other.Count)
                return false;

            return !this
                .Where((t, i) => !t.Equals(other[i]))
                .Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((GeometrySequence<T>)obj);
        }

        public override int GetHashCode()
        {
            return this
                .Select(x => x.GetHashCode())
                .Aggregate(0, (current, result) => (current * 397) ^ result);
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
