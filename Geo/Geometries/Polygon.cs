using System.Text;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : IGeometry, IWktShape, IWktPart
    {
        protected Polygon()
        {
        }

        public Polygon(LinearRing shell)
        {
            Shell = shell;
        }

        public LinearRing Shell { get; protected set; }

        public bool IsEmpty
        {
            get { return Shell.Points.Count < 3; }
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
                buf.Append(")");
            }
            return buf.ToString();
        }
    }
}
