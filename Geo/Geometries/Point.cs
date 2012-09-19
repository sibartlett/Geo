namespace Geo.Geometries
{
    public class Point : LatLngBase<Point>, IPoint
    {
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

        public Envelope GetBounds()
        {
            return new Envelope(Latitude, Longitude, Latitude, Longitude);
        }

        public string ToWktString()
        {
            return string.Format("POINT ({0})", ToWktPartString());
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
