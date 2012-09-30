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
            Coordinates = new CoordinateSequence();
        }

        public LinearRing(IEnumerable<Coordinate> items)
        {
            Coordinates = new CoordinateSequence(items);
        }

        public CoordinateSequence Coordinates { get; private set; }

        private CoordinateSequence ClosedCoordinates()
        {
            if (IsEmpty() || Coordinates[0].Equals(Coordinates[Coordinates.Count - 1]))
                return Coordinates;

            return new CoordinateSequence(Coordinates) { Coordinates[0] };
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
            return Coordinates.ToWktPartString();
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

        protected bool Equals(LinearRing other)
        {
            return Equals(Coordinates, other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LinearRing) obj);
        }

        public override int GetHashCode()
        {
            return (Coordinates != null ? Coordinates.GetHashCode() : 0);
        }

        public static bool operator ==(LinearRing left, LinearRing right)
        {
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(LinearRing left, LinearRing right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null) || !left.Equals(right);
        }
    }
}
