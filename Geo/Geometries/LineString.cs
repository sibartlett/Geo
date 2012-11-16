using System.Collections.Generic;
using System.Linq;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LineString : SpatialObject<LineString>, IGeometry, IWktGeometry, IGeoJsonGeometry
    {
        public static readonly LineString Empty = new LineString();

        public LineString(IEnumerable<Coordinate> coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LineString(params Coordinate[] coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LineString(CoordinateSequence coordinates)
        {
            Coordinates = coordinates;
        }

        public CoordinateSequence Coordinates { get; private set; }

        public Envelope GetBounds()
        {
            return IsEmpty ? null :
                new Envelope(Coordinates.Min(x => x.Latitude), Coordinates.Min(x => x.Longitude), Coordinates.Max(x => x.Latitude), Coordinates.Max(x => x.Longitude));
        }

        public bool IsEmpty
        {
            get { return Coordinates.IsEmpty; }
        }

        public bool HasElevation
        {
            get { return Coordinates.HasElevation; }
        }

        public bool HasM
        {
            get { return Coordinates.HasM; }
        }

        public bool IsClosed
        {
            get
            {
                return !IsEmpty && Coordinates[0].Equals(Coordinates[Coordinates.Count - 1]);
            }
        }

        public Area GetArea()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateArea(Coordinates);
        }

        public Distance GetLength()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLength(Coordinates);
        }

        public string ToWktString()
        {
            return new WktWriter().Write(this);
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }

        public Coordinate this[int index]
        {
            get { return Coordinates[index]; }
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

        #region Equality methods

        public override bool Equals(LineString other, SpatialEqualityOptions options)
        {
            return !ReferenceEquals(null, other) && Equals(Coordinates, other.Coordinates, options);
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
            return (Coordinates != null ? Coordinates.GetHashCode(options) : 0);
        }

        public static bool operator ==(LineString left, LineString right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(LineString left, LineString right)
        {
            return !(left == right);
        }

        #endregion
    }
}
