using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Json;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LineString : IGeometry, IWktGeometry, IWktPart, IGeoJsonGeometry, IEquatable<LineString>
    {
        public LineString()
        {
            Coordinates = new CoordinateSequence();
        }

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

        public bool IsClosed()
        {
            return !IsEmpty && Coordinates[0].Equals(Coordinates[Coordinates.Count - 1]);
        }

        public Envelope GetBounds()
        {
            return IsEmpty ? null :
                new Envelope(Coordinates.Min(x => x.Latitude), Coordinates.Min(x => x.Longitude), Coordinates.Max(x => x.Latitude), Coordinates.Max(x => x.Longitude));
        }

        public bool IsEmpty { get { return Coordinates.Count == 0; } }
        public bool HasElevation { get { return Coordinates.HasElevation; } }
        public bool HasM { get { return Coordinates.HasM; } }

        public Area GetArea()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateArea(Coordinates);
        }

        public Distance GetLength()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLength(Coordinates);
        }

        string IWktPart.ToWktPartString()
        {
            return ((IWktPart) Coordinates).ToWktPartString();
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("LINESTRING ");
            buf.Append(((IWktPart) this).ToWktPartString());
            return buf.ToString();
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

        public bool Equals(LineString other)
        {
            return !ReferenceEquals(null, other) && Equals(Coordinates, other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LineString) obj);
        }

        public override int GetHashCode()
        {
            return (Coordinates != null ? Coordinates.GetHashCode() : 0);
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
