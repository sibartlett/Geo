using System;

namespace Geo.Geometries
{
    public class LatLngCoordinate : ILatLngCoordinate
    {
        protected LatLngCoordinate()
        {
        }

        public LatLngCoordinate(double latitude, double longitude)
        {
            Validate(latitude, longitude);
            Latitude = latitude;
            Longitude = longitude;
        }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }

        public override string ToString()
        {
            return Latitude + ", " + Longitude;
        }

        protected bool Equals(LatLngCoordinate other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
        }

        internal static void Validate(double latitude, double longitude)
        {
            if (latitude > 90 || latitude < -90)
                throw new ArgumentOutOfRangeException("latitude");

            if (longitude > 180 || longitude < -180)
                throw new ArgumentOutOfRangeException("longitude");
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((LatLngCoordinate) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Latitude.GetHashCode()*397) ^ Longitude.GetHashCode();
            }
        } 

        public static LatLngCoordinate Parse(string coordinate)
        {
            if (coordinate == null)
                throw new ArgumentNullException("coordinate");

            if (string.IsNullOrWhiteSpace(coordinate))
                throw new ArgumentException("Value was empty", "coordinate");

            LatLngCoordinate result;
            if (!TryParse(coordinate, out result))
                throw new FormatException("Coordinate (" + coordinate + ") is a not supported format.");

            return result;
        }

        public static LatLngCoordinate TryParse(string coordinate)
        {
            LatLngCoordinate result;
            TryParse(coordinate, out result);
            return result;
        }

        public static bool TryParse(string coordinate, out LatLngCoordinate result)
        {
            var a = GeoUtil.SplitCoordinateString(coordinate);
            if (a != null)
            {
                double lat;
                double lon;
                if (GeoUtil.TryParseOrdinateInternal(a.Item1, GeoUtil.OrdinateType.Latitude, out lat))
                    if (GeoUtil.TryParseOrdinateInternal(a.Item2, GeoUtil.OrdinateType.Longitude, out lon))
                    {
                        result = new LatLngCoordinate(lat, lon);
                        return true;
                    }
            }
            result = default(LatLngCoordinate);
            return false;
        }

        public Point ToPoint()
        {
            return new Point(this);
        }

        public Point ToPoint(double z)
        {
            return new Point(this, z);
        }
    }
}
