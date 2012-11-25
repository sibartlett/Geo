using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.IO.GeoJson;
using Geo.IO.Spatial4n;
using Geo.IO.Wkb;
using Geo.IO.Wkt;

namespace Geo.Geometries
{
    public class Point : SpatialObject, IGeometry, IPosition, IGeoJsonGeometry, IOgcGeometry
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

        public string ToWktString(WktWriterSettings settings)
        {
            return new WktWriter(settings).Write(this);
        }

        public byte[] ToWkbBinary()
        {
            return new WkbWriter().Write(this);
        }

        public byte[] ToWkbBinary(WkbWriterSettings settings)
        {
            return new WkbWriter(settings).Write(this);
        }

        string ISpatial4nShape.ToSpatial4nString()
        {
            return new Spatial4nWriter().Write(this);
        }

        ISpatial4nShape IRavenIndexable.GetSpatial4nShape()
        {
            return this;
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

        #region Equality methods

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as Point;
            return !ReferenceEquals(null, other) && Equals(Coordinate, other.Coordinate, options);
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
