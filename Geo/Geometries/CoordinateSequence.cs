using System.Collections.Generic;
using System.Linq;

namespace Geo.Geometries
{
    public class CoordinateSequence : SpatialReadOnlyCollection<CoordinateSequence, Coordinate>
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
                return Count > 2 && this[0].Equals(this[Count - 1]);
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

        #region Equality methods

        public override bool Equals(CoordinateSequence other, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (Count != other.Count)
                return false;

            return !this
                .Where((t, i) => !SpatialEquality.Equals(t, other[i], options))
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
