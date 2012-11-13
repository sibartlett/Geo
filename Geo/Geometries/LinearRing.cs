using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LinearRing : IGeometry, IWktGeometry, IWktPart, IEquatable<LinearRing>
    {
        public LinearRing()
        {
            Coordinates = new CoordinateSequence();
        }

        public LinearRing(IEnumerable<Coordinate> coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LinearRing(params Coordinate[] coordinates) : this(new CoordinateSequence(coordinates))
        {
        }

        public LinearRing(CoordinateSequence coordinates)
        {
            if (!coordinates.IsClosed)
                throw new ArgumentException("The Coordinate Sequence must be closed to form a Linear Ring");

            Coordinates = coordinates;
        }

        public CoordinateSequence Coordinates { get; private set; }

        public Distance GetLength()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLength(Coordinates);
        }

        public bool IsClosed()
        {
            return !IsEmpty;
        }

        public Envelope GetBounds()
        {
            return IsEmpty ? null :
                new Envelope(Coordinates.Min(x => x.Latitude), Coordinates.Min(x => x.Longitude), Coordinates.Max(x => x.Latitude), Coordinates.Max(x => x.Longitude));
        }

        public Area GetArea()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateArea(Coordinates);
        }

        public bool IsEmpty { get { return Coordinates.Count == 0; } }
        public bool HasElevation { get { return Coordinates.HasElevation; } }
        public bool HasM { get { return Coordinates.HasM; } }

        string IWktPart.ToWktPartString()
        {
            return ((IWktPart) Coordinates).ToWktPartString();
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("LINEARRING ");
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

        #region Equality methods

        public bool Equals(LinearRing other)
        {
            return !ReferenceEquals(null, other) && Equals(Coordinates, other.Coordinates);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((LinearRing) obj);
        }

        public override int GetHashCode()
        {
            return (Coordinates != null ? Coordinates.GetHashCode() : 0);
        }

        public static bool operator ==(LinearRing left, LinearRing right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(LinearRing left, LinearRing right)
        {
            return !(left == right);
        }

        #endregion
    }
}
