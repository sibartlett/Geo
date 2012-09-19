using System;
using System.Globalization;
using Geo.Geometries;

namespace Geo
{
    public abstract class LatLngBase<T> : ILatLng, IWktPart where T : LatLngBase<T>
    {
        internal LatLngBase(double latitude, double longitude)
        {
            if (latitude > 90 || latitude < -90)
                throw new ArgumentOutOfRangeException("latitude");

            if (longitude > 180 || longitude < -180)
                throw new ArgumentOutOfRangeException("longitude");

            Latitude = latitude;
            Longitude = longitude;
        }

        internal LatLngBase(double latitude, double longitude, double elevation) : this(latitude, longitude)
        {
            Elevation = elevation;
        }

        internal LatLngBase(Coordinate coordinate) : this(coordinate.Latitude, coordinate.Longitude)
        {
            Elevation = coordinate.Elevation;
        }

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }
        public double? Elevation { get; private set; }

        public override string ToString()
        {
            var result = Latitude + ", " + Longitude;
            if (Elevation.HasValue)
                result += ", " + Elevation.Value;
            return result;
        }

        public string ToWktPartString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", Longitude, Latitude);
        }

        protected bool Equals(T other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) && Elevation.Equals(other.Elevation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((T)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Latitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Longitude.GetHashCode();
                hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
                return hashCode;
            }
        }
    }
}
