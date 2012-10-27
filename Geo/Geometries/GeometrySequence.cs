using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class GeometrySequence<T> : IEnumerable<T> where T : IGeometry
    {
        private readonly List<T> _geometries;

        public GeometrySequence()
        {
            _geometries = new List<T>();
        }

        public GeometrySequence(IEnumerable<T> geometries)
        {
            _geometries = new List<T>(geometries);
        }

        public GeometrySequence(params T[] geometries)
        {
            _geometries = new List<T>(geometries);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _geometries.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public T this[int index]
        {
            get { return _geometries[index]; }
        }

        public bool IsEmpty
        {
            get { return _geometries.Count == 0; }
        }

        public int Count
        {
            get { return _geometries.Count; }
        }

        #region Equality methods

        protected bool Equals(GeometrySequence<T> other)
        {
            if (other == null)
                return false;

            if (_geometries.Count != other._geometries.Count)
                return false;

            return !_geometries
                .Where((t, i) => !t.Equals(other._geometries[i]))
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
            return _geometries
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
