using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class CoordinateSequence : ReadOnlyCollection<Coordinate>, IWktPart, IEquatable<CoordinateSequence>
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

        string IWktPart.ToWktPartString()
        {
            var buf = new StringBuilder();
            if (IsEmpty)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                var parts = this.Cast<IWktPart>().Select(x => x.ToWktPartString()).ToList();
                string last = null;
                foreach (var part in parts)
                {
                    if (last == null || part != last)
                    {
                        if (last !=null)
                            buf.Append(", ");
                        last = part;
                        buf.Append(part);
                    }
                }
                buf.Append(")");
            }
            return buf.ToString();
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

        public bool Equals(CoordinateSequence other)
        {
            if (ReferenceEquals(null, other))
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
            return Equals((CoordinateSequence)obj);
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
