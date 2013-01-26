using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.IO.Spatial4n;
using Geo.Measure;

namespace Geo
{
    public class Envelope : ISpatial4nShape, IHasArea, IHasLength, IEquatable<Envelope>
    {
        public Envelope(double minLat, double minLon, double maxLat, double maxLon)
        {
            MinLat = minLat;
            MinLon = minLon;
            MaxLat = maxLat;
            MaxLon = maxLon;
        }

        public double MinLat { get; private set; }
        public double MinLon { get; private set; }
        public double MaxLat { get; private set; }
        public double MaxLon { get; private set; }

        public Envelope Combine(Envelope other)
        {
            if (other == null)
                return this;

            return new Envelope(
                Math.Min(MinLat, other.MinLat),
                Math.Min(MinLon, other.MinLon),
                Math.Min(MaxLat, other.MaxLat),
                Math.Min(MaxLon, other.MaxLon)
            );
        }

        public Area GetArea()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateArea(this);
        }

        public Distance GetLength()
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLength(this);
        }

        string ISpatial4nShape.ToSpatial4nString()
        {
            return new Spatial4nWriter().Write(this);
        }

        public bool Intersects(Envelope envelope)
        {
            return envelope.GetExtremeCoordinates().Any(Contains)
                || GetExtremeCoordinates().Any(envelope.Contains);
        }

        public bool Contains(Envelope envelope)
        {
            return envelope != null
                && envelope.MinLat > MinLat
                && envelope.MaxLat < MaxLat
                && envelope.MinLon > MinLon
                && envelope.MaxLon < MaxLon;
        }

        public bool Contains(Coordinate coordinate)
        {
            return coordinate.Latitude > MinLat
                && coordinate.Latitude < MaxLat
                && coordinate.Longitude > MinLon
                && coordinate.Longitude < MaxLon;
        }

        public bool Contains(IGeometry geometry)
        {
            return geometry != null && Contains(geometry.GetBounds());
        }

        private IEnumerable<Coordinate> GetExtremeCoordinates()
        {
            return new[] {
                new Coordinate(MinLat, MinLon),
                new Coordinate(MaxLat, MinLon),
                new Coordinate(MaxLat, MaxLon),
                new Coordinate(MinLat, MaxLon),
                new Coordinate(MinLat, MinLon)
            };
        }

        #region Equality methods

        public bool Equals(Envelope other)
        {
            return !ReferenceEquals(null, other) && MinLat.Equals(other.MinLat) && MinLon.Equals(other.MinLon) && MaxLat.Equals(other.MaxLat) && MaxLon.Equals(other.MaxLon);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Envelope) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = MinLat.GetHashCode();
                hashCode = (hashCode*397) ^ MinLon.GetHashCode();
                hashCode = (hashCode*397) ^ MaxLat.GetHashCode();
                hashCode = (hashCode*397) ^ MaxLon.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(Envelope left, Envelope right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Envelope left, Envelope right)
        {
            return !(left == right);
        }

        #endregion
    }
}
