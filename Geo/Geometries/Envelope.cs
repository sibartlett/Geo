using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class Envelope : IGeometry, IWktShape
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

        public Envelope GetBounds()
        {
            return this;
        }

        string IRavenIndexable.GetIndexString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6} {2:F6} {3:F6}", MinLon, MinLat, MaxLon, MaxLat);
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
                && envelope.MaxLat < MaxLat;
        }

        public bool Contains(Coordinate coordinate)
        {
            return coordinate.Latitude > MinLat
                && coordinate.Latitude < MaxLat
                && coordinate.Longitude > MinLon
                && coordinate.Longitude < MaxLat;
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

        public Polygon ToPolygon()
        {
            return new Polygon(new LinearRing(GetExtremeCoordinates()));
        }

        public string ToWktString()
        {
            return ToPolygon().ToWktString();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (Envelope) obj;
            return MinLat.Equals(other.MinLat) && MinLon.Equals(other.MinLon) && MaxLat.Equals(other.MaxLat) && MaxLon.Equals(other.MaxLon);
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
    }
}
