using System;
using System.Collections.Generic;
using System.Globalization;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Circle : SpatialObject, ISurface
    {
        public static readonly Circle Empty = new Circle();

        public Circle()
        {
            Center = null;
            Radius = 0;
        }

        public Circle(Coordinate center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public Circle(double latitiude, double longitude, double radius)
        {
            Center = new Coordinate(latitiude, longitude);
            Radius = radius;
        }

        public Circle(double latitiude, double longitude, double elevation, double radius)
        {
            Center = new Coordinate(latitiude, longitude, elevation);
            Radius = radius;
        }

        public Circle(double latitiude, double longitude, double elevation, double m, double radius)
        {
            Center = new Coordinate(latitiude, longitude, m, elevation);
            Radius = radius;
        }

        public Coordinate Center { get; private set; }
        public double Radius { get; private set; }

        public Envelope GetBounds()
        {
            var radiusDeg = Radius / (Constants.NauticalMile * 60);

            return new Envelope(
                Center.Latitude - radiusDeg,
                Center.Longitude - radiusDeg,
                Center.Latitude + radiusDeg,
                Center.Longitude + radiusDeg
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

        public bool IsEmpty
        {
            get { return Center == null; }
        }

        public bool HasElevation
        {
            get { return Center != null && Center.HasElevation; }
        }

        public bool HasM
        {
            get { return Center != null && Center.HasM; }
        }

        string IRavenIndexable.GetIndexString()
        {
            return string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F6} {1:F6} d={2:F6})", Center.Longitude, Center.Latitude, Radius.ConvertTo(DistanceUnit.Km));
        }

        public Polygon ToPolygon(int sides = 36)
        {
            if (sides < 3)
                throw new ArgumentOutOfRangeException("sides", "Must be greater than 2.");

            var angle = -360d / sides;
            var coordinates = new List<Coordinate>();
            Coordinate first = null;
            for (var i = 0; i < sides; i++)
            {
                var coord = GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(Center, angle * i, Radius).Coordinate2;
                if (i == 0)
                    first = coord;
                coordinates.Add(coord);
            }
            coordinates.Add(first);
            return new Polygon(new LinearRing(coordinates));
        }

        #region Equality methods

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as Circle;
            return !ReferenceEquals(null, other) && Radius.Equals(other.Radius) && Equals(Center, other.Center, options);
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
            unchecked
            {
                return (Radius.GetHashCode() * 397) ^ (Center != null ? Center.GetHashCode(options) : 0);
            }
        }

        public static bool operator ==(Circle left, Circle right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Circle left, Circle right)
        {
            return !(left == right);
        }

        #endregion
    }
}
