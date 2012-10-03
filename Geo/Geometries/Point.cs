using System.Globalization;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class Point : LatLngBase<Point>, IPoint
    {
        public override int GetHashCode()
        {
            throw new System.NotImplementedException();
        }

        protected Point() : base(0, 0)
        {
        }

        public Point(double latitude, double longitude) : base(latitude, longitude)
        {
        }

        public Point(double latitude, double longitude, double elevation) : base(latitude, longitude, elevation)
        {
        }

        public Point(Coordinate coordinate) : base(coordinate)
        {
        }

        public Coordinate GetCoordinate()
        {
            if (Elevation.HasValue)
                return new Coordinate(Latitude, Longitude, Elevation.Value);
            return new Coordinate(Latitude, Longitude);
        }

        public string ToWktString()
        {
            return string.Format("POINT ({0})", ToWktPartString());
        }

        string IRavenIndexable.GetIndexString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", Longitude, Latitude);
        }

        public static Point Parse(string coordinate)
        {
            return Coordinate.Parse(coordinate).ToPoint();
        }

        public static Point TryParse(string coordinate)
        {
            Coordinate result;
            var success = Coordinate.TryParse(coordinate, out result);
            return success ? result.ToPoint() : default(Point);
        }

        public static bool TryParse(string coordinate, out Point result)
        {
            Coordinate res;
            var success = Coordinate.TryParse(coordinate, out res);
            result = success ? res.ToPoint() : default(Point);
            return success;
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public static bool operator ==(Point left, Point right)
        {
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null) || !left.Equals(right);
        }
    }
}
