using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LinearRing : LinearRing<Point>
    {
        public LinearRing()
        {
        }

        public LinearRing(IEnumerable<Point> items)
            : base(items)
        {
        }
    }

    public class LinearRing<T> : IGeometry, IWktShape, IWktPart where T : class, IPoint
    {
        public LinearRing()
        {
            Points = new List<T>();
        }

        public LinearRing(IEnumerable<T> items)
        {
            Points = new List<T>(items);
        }

        public List<T> Points { get; protected set; }

        public Distance CalculatePerimeter()
        {
            var distance = new Distance(0);

            if (!IsEmpty)
            {
                for (var i = 1; i < Points.Count; i++)
                {
                    var line = Points[i - 1].CalculateShortestLine(Points[i]);
                    if (line != null)
                        distance += line.Distance;

                    if (i == Points.Count - 1)
                    {
                        line = Points[0].CalculateShortestLine(Points[i - 1]);
                        if (line != null)
                            distance += line.Distance;
                    }
                }
            }

            return distance;
        } 

        public bool IsEmpty
        {
            get { return Points.Count == 0; }
        }

        public bool IsClosed
        {
            get { return !IsEmpty && Points[0] == Points[Points.Count - 1]; }
        }

        public Bounds GetBounds()
        {
            return IsEmpty ? null :
                new Bounds(Points.Min(x => x.Latitude), Points.Min(x => x.Longitude), Points.Max(x => x.Latitude), Points.Max(x => x.Longitude));
        }

        public string ToWktPartString()
        {
            var buf = new StringBuilder();
            if (IsEmpty)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                for (var i = 0; i < Points.Count; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(Points[i].ToWktPartString());

                    if (i == Points.Count - 1 && Points[0] != Points[Points.Count - 1])
                    {
                        buf.Append(", ");
                        buf.Append(Points[0].ToWktPartString());
                    }
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("LINEARRING ");
            buf.Append(ToWktPartString());
            return buf.ToString();
        }

        public T this[int index]
        {
            get { return Points[index]; }
        }

        public void Add(T point)
        {
            Points.Add(point);
        }
    }
}
