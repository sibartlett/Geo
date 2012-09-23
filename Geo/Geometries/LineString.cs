using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LineString : IGeometry, IWktShape, IWktPart
    {
        public LineString()
        {
            Coordinates = new List<Coordinate>();
        }

        public LineString(IEnumerable<Coordinate> coordinates)
        {
            Coordinates = new List<Coordinate>(coordinates);
        }

        public List<Coordinate> Coordinates { get; private set; }

        public Distance CalculateLength()
        {
            return Coordinates.CalculateShortestDistance();
        }

        public bool IsEmpty()
        {
            return Coordinates.Count == 0;
        }

        public bool IsClosed()
        {
            return !IsEmpty() && Coordinates[0] == Coordinates[Coordinates.Count - 1];
        }

        public Envelope GetBounds()
        {
            return IsEmpty() ? null :
                new Envelope(Coordinates.Min(x => x.Latitude), Coordinates.Min(x => x.Longitude), Coordinates.Max(x => x.Latitude), Coordinates.Max(x => x.Longitude));
        }

        public string ToWktPartString()
        {
            var buf = new StringBuilder();
            if (IsEmpty())
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                for (var i = 0; i < Coordinates.Count; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(Coordinates[i].ToWktPartString());
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("LINESTRING ");
            buf.Append(ToWktPartString());
            return buf.ToString();
        }

        public Coordinate this[int index]
        {
            get { return Coordinates[index]; }
        }

        public void Add(Coordinate coordinate)
        {
            Coordinates.Add(coordinate);
        }
    }
}
