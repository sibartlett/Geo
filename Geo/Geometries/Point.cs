using System.Globalization;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Point : SpatialObject<Point>, IGeometry, IPosition, IGeoJsonGeometry, IOgcGeometry
    {
        public static readonly Point Empty = new Point();

        public Point() : this(null)
        {
        }

        public Point(double latitude, double longitude)
        {
            Coordinate = new Coordinate(latitude, longitude);
        }

        public Point(double latitude, double longitude, double elevation)
        {
            Coordinate = new Coordinate(latitude, longitude, elevation);
        }

        public Point(double latitude, double longitude, double elevation, double m)
        {
            Coordinate = new Coordinate(latitude, longitude, elevation, m);
        }

        public Point(Coordinate coordinate)
        {
            Coordinate = coordinate;
        }

        public Coordinate Coordinate { get; set; }

        Coordinate IPosition.GetCoordinate()
        {
            return Coordinate;
        }

        public string ToWktString()
        {
            return new WktWriter().Write(this);
        }

        string IRavenIndexable.GetIndexString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", Coordinate.Longitude, Coordinate.Latitude);
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

        public bool IsEmpty
        {
            get { return Coordinate == null; }
        }

        public bool HasElevation
        {
            get { return Coordinate != null && Coordinate.HasElevation; }
        }

        public bool HasM
        {
            get { return Coordinate != null && Coordinate.HasM; }
        }

        public Envelope GetBounds()
        {
            return Coordinate.GetBounds();
        }

        public Area GetArea()
        {
            return new Area(0);
        }

        public Distance GetLength()
        {
            return new Distance(0);
        }

        #region Equality methods

        public override bool Equals(Point other, SpatialEqualityOptions options)
        {
            return !ReferenceEquals(null, other) && Equals(Coordinate, other.Coordinate, options);
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetHashCode(SpatialEqualityOptions options)
        {
            return (Coordinate != null ? Coordinate.GetHashCode(options) : 0);
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
