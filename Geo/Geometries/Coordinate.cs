using System;

namespace Geo.Geometries
{
    public class Coordinate : LatLngBase<Coordinate>
    {
        protected Coordinate() : base(0, 0)
        {
        }

        public Coordinate(double latitude, double longitude) : base(latitude, longitude)
        {
        }

        public Coordinate(double latitude, double longitude, double elevation) : base(latitude, longitude, elevation)
        {
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

        public static implicit operator Coordinate(Point point)
        {
            if(point.Elevation.HasValue)
                return new Coordinate(point.Latitude, point.Longitude, point.Elevation.Value);
            return new Coordinate(point.Latitude, point.Longitude);
        }
    }
}
