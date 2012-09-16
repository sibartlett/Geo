using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LinearRing : LinearRing<Coordinate>
    {
        public LinearRing()
        {
        }

        public LinearRing(IEnumerable<Coordinate> items)
            : base(items)
        {
        }
    }

    public class LinearRing<T> : IGeometry, IWktShape, IWktPart where T : class, ICoordinate
    {
        public LinearRing()
        {
            Coordinates = new List<T>();
        }

        public LinearRing(IEnumerable<T> items)
        {
            Coordinates = new List<T>(items);
        }

        public List<T> Coordinates { get; protected set; }

        public Distance CalculatePerimeter()
        {
            var distance = new Distance(0);

            if (!IsEmpty)
            {
                for (var i = 1; i < Coordinates.Count; i++)
                {
                    var line = Coordinates[i - 1].CalculateShortestLine(Coordinates[i]);
                    if (line != null)
                        distance += line.Distance;

                    if (i == Coordinates.Count - 1)
                    {
                        line = Coordinates[0].CalculateShortestLine(Coordinates[i - 1]);
                        if (line != null)
                            distance += line.Distance;
                    }
                }
            }

            return distance;
        } 

        public bool IsEmpty
        {
            get { return Coordinates.Count == 0; }
        }

        public bool IsClosed
        {
            get { return !IsEmpty && Coordinates[0] == Coordinates[Coordinates.Count - 1]; }
        }

        public Envelope GetBounds()
        {
            return IsEmpty ? null :
                new Envelope(Coordinates.Min(x => x.Latitude), Coordinates.Min(x => x.Longitude), Coordinates.Max(x => x.Latitude), Coordinates.Max(x => x.Longitude));
        }

        public string ToWktPartString()
        {
            var buf = new StringBuilder();
            if (IsEmpty)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                for (var i = 0; i < Coordinates.Count; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(Coordinates[i].ToWktPartString());

                    if (i == Coordinates.Count - 1 && Coordinates[0] != Coordinates[Coordinates.Count - 1])
                    {
                        buf.Append(", ");
                        buf.Append(Coordinates[0].ToWktPartString());
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
            get { return Coordinates[index]; }
        }

        public void Add(T point)
        {
            Coordinates.Add(point);
        }
    }
}
