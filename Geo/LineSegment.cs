using System;
using Geo.Abstractions;

namespace Geo;

public class LineSegment : SpatialObject
{
    public LineSegment(Coordinate coordinate1, Coordinate coordinate2)
    {
        if (coordinate1 == null)
            throw new ArgumentNullException("coordinate1");
        if (coordinate2 == null)
            throw new ArgumentNullException("coordinate2");

        Coordinate1 = coordinate1;
        Coordinate2 = coordinate2;
    }

    public Coordinate Coordinate1 { get; }
    public Coordinate Coordinate2 { get; }

    public Envelope GetBounds()
    {
        return Coordinate1.GetBounds().Combine(Coordinate2.GetBounds());
    }

    #region Equality methods

    public override bool Equals(object obj, SpatialEqualityOptions options)
    {
        var other = obj as LineSegment;
        return !ReferenceEquals(null, other)
            && Equals(Coordinate1, other.Coordinate1, options)
            && Equals(Coordinate2, other.Coordinate2, options);
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            return ((Coordinate1 != null ? Coordinate1.GetHashCode(options) : 0) * 397)
                ^ (Coordinate2 != null ? Coordinate2.GetHashCode(options) : 0);
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

    public static bool operator ==(LineSegment left, LineSegment right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(LineSegment left, LineSegment right)
    {
        return !(left == right);
    }

    #endregion
}
