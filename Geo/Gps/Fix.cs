using System;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.Gps
{
    public class Fix : IPosition
    {
        protected Fix()
        {
        }

        public Fix(double latitude, double longitude, DateTime dateTime)
        {
            Coordinate = new Coordinate(latitude, longitude);
            TimeUtc = dateTime;
        }

        public Fix(double latitude, double longitude, double elevation, DateTime dateTime)
        {
            Coordinate = new CoordinateZ(latitude, longitude, elevation);
            TimeUtc = dateTime;
        }

        public Coordinate Coordinate { get; set; }
        public DateTime TimeUtc { get; set; }

        protected bool Equals(Fix other)
        {
            return Equals(Coordinate, other.Coordinate) && TimeUtc.Equals(other.TimeUtc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Fix) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Coordinate != null ? Coordinate.GetHashCode() : 0)*397) ^ TimeUtc.GetHashCode();
            }
        }

        Coordinate IPosition.GetCoordinate()
        {
            return Coordinate;
        }
    }
}