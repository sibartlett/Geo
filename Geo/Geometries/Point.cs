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
            Coordinate=new Coordinate(latitude, longitude);
        }

        public Point(double latitude, double longitude, double z)
        {
            Coordinate = new Coordinate(latitude, longitude, z);
        }

        public Point(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public Coordinate Coordinate { get; protected set; }
        public double Latitude { get { return Coordinate.Latitude; } }
        public double Longitude { get { return Coordinate.Longitude; } }
        public double? Elevation { get { return Coordinate.Elevation; } }

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

        public string ToWktString()
        {
            return string.Format("POINT ({0})", Coordinate.ToWktPartString());
        }

        public static Point ParseCoordinate(string coordinate)
        {
            return Coordinate.Parse(coordinate).ToPoint();
        }

        public static Point TryParseCoordinate(string coordinate)
        {
            Coordinate result;
            var success = Coordinate.TryParse(coordinate, out result);
            return success ? result.ToPoint() : default(Point);
        }

        public static bool TryParseCoordinate(string coordinate, out Point result)
        {
            Coordinate res;
            var success = Coordinate.TryParse(coordinate, out res);
            result = success ? res.ToPoint() : default(Point);
            return success;
        }
    }
}
