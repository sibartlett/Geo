using System;
using System.Globalization;

namespace Geo.Geometries
{
    public class Coordinate : ICoordinate
    {
        protected Coordinate()
        {
        }

        public Coordinate(double latitude, double longitude)
        {
            Validate(latitude, longitude);
            Latitude = latitude;
            Longitude = longitude;
        }

        public Coordinate(double latitude, double longitude, double z) : this(latitude, longitude)
        {
            Elevation = z;
        }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
        public double? Elevation { get; protected set; }

        public override string ToString()
        {
            return Latitude + ", " + Longitude;
        }

        public string ToWktPartString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", Longitude, Latitude);
        }

        internal static void Validate(double latitude, double longitude)
        {
            if (latitude > 90 || latitude < -90)
                throw new ArgumentOutOfRangeException("latitude");

            if (longitude > 180 || longitude < -180)
                throw new ArgumentOutOfRangeException("longitude");
        }

        protected bool Equals(Coordinate other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) && Elevation.Equals(other.Elevation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Coordinate)obj);
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

        public static Coordinate Parse(string coordinate)
        {
            if (coordinate == null)
                throw new ArgumentNullException("coordinate");

            if (coordinate.IsNullOrWhitespace())
                throw new ArgumentException("Value was empty", "coordinate");

            Coordinate result;
            if (!TryParse(coordinate, out result))
                throw new FormatException("Coordinate (" + coordinate + ") is a not supported format.");

            return result;
        }

        public static Coordinate TryParse(string coordinate)
        {
            Coordinate result;
            TryParse(coordinate, out result);
            return result;
        }

        public static bool TryParse(string coordinate, out Coordinate result)
        {
            var a = GeoUtil.SplitCoordinateString(coordinate);
            if (a != null)
            {
                double lat;
                double lon;
                if (GeoUtil.TryParseOrdinateInternal(a[0], GeoUtil.OrdinateType.Latitude, out lat))
                    if (GeoUtil.TryParseOrdinateInternal(a[1], GeoUtil.OrdinateType.Longitude, out lon))
                    {
                        result = new Coordinate(lat, lon);
                        return true;
                    }
            }
            result = default(Coordinate);
            return false;
        }

        public Point ToPoint()
        {
            return new Point(this);
        }
    }
}
