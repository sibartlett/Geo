using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Circle : IGeometry, IWktGeometry
    {
        public Circle()
        {
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

        public bool IsEmpty { get { return Center != null && !double.IsNaN(Radius); } }
        public bool HasElevation { get { return Center != null && Center.HasElevation; } }
        public bool HasM { get { return Center != null && Center.HasM; } }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("CIRCULARSTRING (");
            string first = null;
            for (var i = 0; i < 4; i++)
            {
                IWktPart coord = GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(Center, 90 * i, Radius).Coordinate2;
                if (i == 0)
                    first = coord.ToWktPartString();
                else
                    buf.Append(", ");
                buf.Append(coord.ToWktPartString());
            }
            buf.Append(first);
            buf.Append(")");
            return buf.ToString();
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
            var coords = new List<Coordinate>();
            Coordinate first = null;
            for (var i = 0; i < sides; i++)
            {
                var coord = GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(Center, angle * i, Radius).Coordinate2;
                if (i == 0)
                    first = coord;
                coords.Add(coord);
            }
            coords.Add(first);
            return new Polygon(new LinearRing(coords));
        }

        #region Equality methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (Circle) obj;
            return Radius.Equals(other.Radius) && Equals(Center, other.Center);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return (Radius.GetHashCode()*397) ^ (Center != null ? Center.GetHashCode() : 0);
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
