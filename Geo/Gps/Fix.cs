using System;
using Geo.Geometries;

namespace Geo.Gps
{
    public class Fix
    {
        protected Fix()
        {
        }

        public Fix(double lat, double lon, DateTime dateTime)
        {
            Coordinate =new Coordinate(lat, lon);
            TimeUtc = dateTime;
        }

        public Fix(double lat, double lon, double z, DateTime dateTime)
        {
            Coordinate = new Coordinate(lat, lon, z);
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
    }
}