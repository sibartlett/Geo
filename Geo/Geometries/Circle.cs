using System.Globalization;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Circle : IGeometry
    {
        protected Circle()
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

        public Coordinate Center { get; private set; }
        public double Radius { get; private set; }

        public Envelope GetBounds()
        {
            var radiusDeg = Radius / (Reference.Ellipsoid.NauticalMile * 60);

            return new Envelope(
                Center.Latitude - radiusDeg,
                Center.Longitude - radiusDeg,
                Center.Latitude + radiusDeg,
                Center.Longitude + radiusDeg
            );
        }

        string IRavenIndexable.GetIndexString()
        {
            return string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F6} {1:F6} d={2:F6})", Center.Longitude, Center.Latitude, Radius.ConvertTo(DistanceUnit.Km));
        }

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
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Circle left, Circle right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null) || !left.Equals(right);
        }
    }
}
