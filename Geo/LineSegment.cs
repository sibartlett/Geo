#nullable enable
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

    /// <remarks>
    /// The runtime types have to match, not merely be compatible. A
    /// <see cref="Geodesy.GeodeticLine" /> is a line segment that also carries a distance
    /// and two bearings, and it compares those; accepting one here made equality
    /// asymmetric, since a segment equalled a geodetic line running between the same two
    /// coordinates while the line did not equal the segment. Anything comparing one against
    /// the other - <c>List.Contains</c>, <c>Remove</c>, <c>IndexOf</c> - then answered
    /// differently depending on which of the two it was holding, and the pair carried
    /// different hash codes while still comparing equal one way round.
    /// </remarks>
    public override bool Equals(object? obj, SpatialEqualityOptions options)
    {
        if (obj == null || obj.GetType() != GetType())
            return false;

        var other = (LineSegment)obj;
        return Equals(Coordinate1, other.Coordinate1, options)
            && Equals(Coordinate2, other.Coordinate2, options);
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            return (Coordinate1.GetHashCode(options) * 397) ^ Coordinate2.GetHashCode(options);
        }
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public static bool operator ==(LineSegment? left, LineSegment? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(LineSegment? left, LineSegment? right)
    {
        return !(left == right);
    }

    #endregion
}
