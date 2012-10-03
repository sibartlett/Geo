using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : IGeometry, IWktShape, IWktPart
    {
        protected Polygon()
        {
            Holes = new List<LinearRing>();
        }

        public Polygon(LinearRing shell, params LinearRing[] holes)
        {
            Shell = shell;
            Holes = new List<LinearRing>(holes ?? new LinearRing[0]);
        }

        public LinearRing Shell { get; private set; }
        public List<LinearRing> Holes { get; private set; }

        public bool IsEmpty()
        {
            return Shell.Coordinates.Count < 3;
        }

        public Distance CalculatePerimeter()
        {
            return Shell.CalculatePerimeter();
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("POLYGON ");
            buf.Append(ToWktPartString());
            return buf.ToString();
        }

        public string ToWktPartString()
        {
            var buf = new StringBuilder();
            if (IsEmpty())
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                buf.Append(Shell.ToWktPartString());
                foreach (var hole in Holes)
                {
                    buf.Append(", ");
                    buf.Append(hole.ToWktPartString());
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        public Envelope GetBounds()
        {
            return Shell.GetBounds();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (Polygon) obj;

            if (Holes.Count != other.Holes.Count)
                return false;

            if (Holes.Where((t, i) => !t.Equals(other.Holes[i])).Any())
                return false;

            return Equals(Shell, other.Shell);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Shell != null ? Shell.GetHashCode() : 0;
                return Holes
                    .Select(x => x.GetHashCode())
                    .Aggregate(hashCode, (current, result) => (current * 397) ^ result);
            }
        }

        public static bool operator ==(Polygon left, Polygon right)
        {
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Polygon left, Polygon right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null) || !left.Equals(right);
        }
    }
}
