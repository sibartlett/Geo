using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Json;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : IGeometry, IWktGeometry, IWktPart, IGeoJsonGeometry
    {
        public Polygon()
        {
            Shell = new LinearRing();
            Holes = new GeometrySequence<LinearRing>();
        }

        public Polygon(LinearRing shell, IEnumerable<LinearRing> holes)
        {
            Shell = shell;
            Holes = new GeometrySequence<LinearRing>(holes ?? new LinearRing[0]);
        }

        public Polygon(LinearRing shell, params LinearRing[] holes)
        {
            Shell = shell;
            Holes = new GeometrySequence<LinearRing>(holes ?? new LinearRing[0]);
        }

        public LinearRing Shell { get; private set; }
        public GeometrySequence<LinearRing> Holes { get; private set; }

        public bool IsEmpty { get { return Shell.Coordinates.Count == 0; } }
        public bool HasElevation { get { return Shell.Coordinates.HasElevation; } }
        public bool HasM { get { return Shell.Coordinates.HasM; } }

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
            if (IsEmpty)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                buf.Append(((IWktPart) Shell).ToWktPartString());
                foreach (var hole in Holes.Cast<IWktPart>())
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

        public Area GetArea()
        {
            return Shell.GetArea() -  new Area(Holes.Sum(x => x.GetArea().SiValue));
        }

        public string ToGeoJson()
        {
            return SimpleJson.SerializeObject(this.ToGeoJsonObject());
        }

        public object ToGeoJsonObject()
        {
            return new Dictionary<string, object>
            {
                { "type", "Polygon" },
                { "coordinates", this.ToCoordinateArray() }
            };
        }

        #region Equality methods

        protected bool Equals(Polygon other)
        {
            return Equals(Shell, other.Shell) && Equals(Holes, other.Holes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Polygon) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Shell != null ? Shell.GetHashCode() : 0)*397) ^ (Holes != null ? Holes.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Polygon left, Polygon right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Polygon left, Polygon right)
        {
            return !(left == right);
        }

        #endregion
    }
}
