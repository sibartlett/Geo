using System.Collections.Generic;
using System.Globalization;
using Geo.Interfaces;
using Geo.Json;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Point : LatLngBase<Point>, IPoint, IGeoJsonGeometry
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

        public Coordinate GetCoordinate()
        {
            if (Elevation.HasValue)
                return new Coordinate(Latitude, Longitude, Elevation.Value);
            return new Coordinate(Latitude, Longitude);
        }

        public string ToWktString()
        {
            return string.Format("POINT ({0})", ((IWktPart) this).ToWktPartString());
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

        public string ToGeoJson()
        {
            return SimpleJson.SerializeObject(this.ToGeoJsonObject());
        }

        public object ToGeoJsonObject()
        {
            return new Dictionary<string, object>
            {
                { "type", "Point" },
                { "coordinates", this.ToCoordinateArray() }
            };
        }

        public Area GetArea()
        {
            return new Area(0d);
        }

        #region Equality methods

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public static bool operator ==(Point left, Point right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Point left, Point right)
        {
            return !(left == right);
        }

        #endregion
    }
}
