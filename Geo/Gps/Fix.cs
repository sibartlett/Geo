using System;
using Geo.Geometries;

namespace Geo.Gps
{
    public class Fix : Point
    {
        public Fix(double lat, double lon, DateTime dateTime) : base(lat, lon)
        {
            TimeUtc = dateTime;
        }

        public Fix(double lat, double lon, double z, DateTime dateTime) : base(lat, lon, z)
        {
            TimeUtc = dateTime;
        }

        public DateTime TimeUtc { get; set; }

        protected bool Equals(Fix other)
        {
            return base.Equals(other) && TimeUtc.Equals(other.TimeUtc);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Fix) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (base.GetHashCode()*397) ^ TimeUtc.GetHashCode();
            }
        }
    }
}