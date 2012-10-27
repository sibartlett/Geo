using Geo.Measure;

namespace Geo.Geometries
{
    public class GeodeticLine
    {
        public GeodeticLine(Coordinate coordinate1, Coordinate coordinate2, double distance, double bearing12, double bearing21)
        {
            Coordinate1 = coordinate1;
            Coordinate2 = coordinate2;
            Bearing12 = bearing12.NormalizeDegrees();
            Bearing21 = bearing21.NormalizeDegrees();
            Distance = new Distance(distance);
        }

        public Coordinate Coordinate1 { get; private set; }
        public Coordinate Coordinate2 { get; private set; }
        public Distance Distance { get; private set; }
        public double Bearing12 { get; private set; }
        public double Bearing21 { get; private set; }

        #region Equality methods

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            var other = (GeodeticLine) obj;
            return Equals(Coordinate1, other.Coordinate1) && Equals(Coordinate2, other.Coordinate2) && Distance.Equals(other.Distance) && Bearing12.Equals(other.Bearing12) && Bearing21.Equals(other.Bearing21);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = (Coordinate1 != null ? Coordinate1.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ (Coordinate2 != null ? Coordinate2.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ Distance.GetHashCode();
                hashCode = (hashCode*397) ^ Bearing12.GetHashCode();
                hashCode = (hashCode*397) ^ Bearing21.GetHashCode();
                return hashCode;
            }
        }

        public static bool operator ==(GeodeticLine left, GeodeticLine right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(GeodeticLine left, GeodeticLine right)
        {
            return !(left == right);
        }

        #endregion
    }
}
