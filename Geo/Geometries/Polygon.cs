using System.Collections.Generic;
using System.Text;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : IGeometry, IWktShape, IWktPart
    {
        protected Polygon()
        {
            Holes= new List<LinearRing>();
        }

        public Polygon(LinearRing shell, params LinearRing[] holes)
        {
            Shell = shell;
            Holes = new List<LinearRing>(holes ?? new LinearRing[0]);
        }

        public LinearRing Shell { get; protected set; }
        public List<LinearRing> Holes { get; protected set; }

        public bool IsEmpty
        {
            get { return Shell.Coordinates.Count < 3; }
        }

        public Distance CalculatePerimeter()
        {
            return Shell.CalculatePerimeter();
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
    }
}
