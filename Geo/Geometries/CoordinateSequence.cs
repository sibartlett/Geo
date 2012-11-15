using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class CoordinateSequence : ReadOnlyCollection<Coordinate>, ISpatialEquatable<CoordinateSequence>
    {
        public CoordinateSequence() : base(new List<Coordinate>())
        {
        }

        public CoordinateSequence(IEnumerable<Coordinate> coordinates) : base(coordinates.ToList())
        {
        }

        public CoordinateSequence(params Coordinate[] coordinates) : base(coordinates.ToList())
        {
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public bool HasElevation
        {
            get { return this.Any(x => x.HasElevation); }
        }

        public bool HasM
        {
            get { return this.Any(x => x.HasM); }
        }

        public bool IsClosed
        {
            get
            {
                return Count > 1 && this[0].Equals(this[Count - 1]);
            }
        }

        public IEnumerable<LineSegment> ToLineSegments()
        {
            Coordinate last = null;
            foreach (var coordinate in this)
            {
                if (last != null)
                    yield return new LineSegment(last, coordinate);
                last = coordinate;
            }
        }

        public LineString ToLineString()
        {
            return new LineString(this);
        }

        public LinearRing ToLinearRing()
        {
            return new LinearRing(this);
        }

        #region Equality methods

        public bool Equals(CoordinateSequence other, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (Count != other.Count)
                return false;

            return !this
                .Where((t, i) => !SpatialEquality.Equals(t, other[i], options))
                .Any();
        }

        public bool Equals(CoordinateSequence other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions);
        }

        public bool Equals(object obj, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((CoordinateSequence)obj, options);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, GeoContext.Current.EqualityOptions);
        }

        public override int GetHashCode()
        {
            return this
                .Select(x => x.GetHashCode())
                .Aggregate(0, (current, result) => (current * 397) ^ result);
        }

        public static bool operator ==(CoordinateSequence left, CoordinateSequence right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(CoordinateSequence left, CoordinateSequence right)
        {
            return !(left == right);
        }

        #endregion
    }
}
