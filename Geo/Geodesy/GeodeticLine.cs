using Geo.Measure;

namespace Geo.Geodesy;

public class GeodeticLine : LineSegment
{
    public GeodeticLine(
        Coordinate coordinate1,
        Coordinate coordinate2,
        double distance,
        double bearing12,
        double bearing21
    )
        : base(coordinate1, coordinate2)
    {
        Bearing12 = bearing12.NormalizeDegrees();
        Bearing21 = bearing21.NormalizeDegrees();
        Distance = new Distance(distance);
    }

    public Distance Distance { get; }
    public double Bearing12 { get; }
    public double Bearing21 { get; }

    #region Equality methods

    public override bool Equals(object obj, SpatialEqualityOptions options)
    {
        var other = obj as GeodeticLine;
        return !ReferenceEquals(null, other)
            && Equals(Coordinate1, other.Coordinate1, options)
            && Equals(Coordinate2, other.Coordinate2, options)
            && Distance.Equals(other.Distance)
            && Bearing12.Equals(other.Bearing12)
            && Bearing21.Equals(other.Bearing21);
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            var hashCode = Coordinate1 != null ? Coordinate1.GetHashCode(options) : 0;
            hashCode =
                (hashCode * 397) ^ (Coordinate2 != null ? Coordinate2.GetHashCode(options) : 0);
            hashCode = (hashCode * 397) ^ Distance.GetHashCode();
            hashCode = (hashCode * 397) ^ Bearing12.GetHashCode();
            hashCode = (hashCode * 397) ^ Bearing21.GetHashCode();
            return hashCode;
        }
    }

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
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
