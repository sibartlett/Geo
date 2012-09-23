using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LinearRing : IGeometry, IWktShape, IWktPart
    {
        public LinearRing()
        {
            Coordinates = new List<Coordinate>();
        }

        public LinearRing(IEnumerable<Coordinate> items)
        {
            Coordinates = new List<Coordinate>(items);
        }

        public List<Coordinate> Coordinates { get; private set; }

        private List<Coordinate> ClosedCoordinates()
        {
            if(IsEmpty() || Coordinates[0] == Coordinates[Coordinates.Count - 1])
                return Coordinates;

            var coordinates = Coordinates.ToList();
            coordinates.Add(Coordinates[0]);
            return coordinates;
        }

        public Distance CalculatePerimeter()
        {
            return ClosedCoordinates().CalculateShortestDistance();
        } 

        public bool IsEmpty()
        {
            return Coordinates.Count == 0;
        }

        public bool IsClosed()
        {
            return !IsEmpty();
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
                var coordinates = ClosedCoordinates();
                for (var i = 0; i < coordinates.Count; i++)
                {
                    if (i > 0)
                        buf.Append(", ");
                    buf.Append(coordinates[i].ToWktPartString());
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

        public Coordinate this[int index]
        {
            get { return Coordinates[index]; }
        }

        public void Add(Coordinate coordinates)
        {
            Coordinates.Add(coordinates);
        }
    }
}
