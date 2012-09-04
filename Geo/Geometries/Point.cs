using System.Globalization;

namespace Geo.Geometries
{
    public class Point : IPoint
    {
        protected Point()
        {
        }

        public Point(double latitude, double longitude)
        {
            LatLngCoordinate.Validate(latitude, longitude);
            Latitude = latitude;
            Longitude = longitude;
        }

        public Point(double latitude, double longitude, double z) : this(latitude, longitude)
        {
            Elevation = z;
        }

        public Point(ILatLngCoordinate coordinate) : this(coordinate.Latitude, coordinate.Longitude)
        {
        }

        public Point(ILatLngCoordinate coordinate, double z) : this(coordinate.Latitude, coordinate.Longitude, z)
        {
        }

        public double Latitude { get; protected set; }
        public double Longitude { get; protected set; }
        public double? Elevation { get; protected set; }

        public override string ToString()
        {
            if(Elevation == null)
                return Latitude + ", " + Longitude;
            return Latitude + ", " + Longitude + ", " + Elevation.Value;
        }

        protected bool Equals(Point other)
        {
            return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude) && Elevation.Equals(other.Elevation);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((Point) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = Latitude.GetHashCode();
                hashCode = (hashCode*397) ^ Longitude.GetHashCode();
                hashCode = (hashCode*397) ^ Elevation.GetHashCode();
                return hashCode;
            }
        }

        public string ToWktPartString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", Longitude, Latitude);
        }

        public string ToWktString()
        {
            return string.Format("POINT ({0})", ToWktPartString());
        }

        public static Point ParseCoordinate(string coordinate)
        {
            return LatLngCoordinate.Parse(coordinate).ToPoint();
        }

        public static Point TryParseCoordinate(string coordinate)
        {
            LatLngCoordinate result;
            var success = LatLngCoordinate.TryParse(coordinate, out result);
            return success ? result.ToPoint() : default(Point);
        }

        public static bool TryParseCoordinate(string coordinate, out Point result)
        {
            LatLngCoordinate res;
            var success = LatLngCoordinate.TryParse(coordinate, out res);
            result = success ? res.ToPoint() : default(Point);
            return success;
        }
    }
}
