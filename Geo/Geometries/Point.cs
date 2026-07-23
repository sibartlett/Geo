using Geo.Abstractions;
using Geo.Abstractions.Interfaces;

namespace Geo.Geometries;

public class Point : Geometry, IPosition
{
    public static readonly Point Empty = new();

    public Point()
        : this(null) { }

    public Point(double latitude, double longitude)
    {
        Coordinate = new Coordinate(latitude, longitude);
    }

    public Point(double latitude, double longitude, double elevation)
    {
        Coordinate = new CoordinateZ(latitude, longitude, elevation);
    }

    public Point(double latitude, double longitude, double elevation, double measure)
    {
        Coordinate = new CoordinateZM(latitude, longitude, elevation, measure);
    }

    public Point(Coordinate coordinate)
    {
        Coordinate = coordinate;
    }

    public Coordinate Coordinate { get; set; }

    public override bool IsEmpty => Coordinate == null;

    public override bool Is3D => Coordinate != null && Coordinate.Is3D;

    public override bool IsMeasured => Coordinate != null && Coordinate.IsMeasured;

    public override Envelope GetBounds()
    {
        return Coordinate.GetBounds();
    }

    #region Equality methods

    public override bool Equals(object obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override bool Equals(object obj, SpatialEqualityOptions options)
    {
        var other = obj as Point;
        return !ReferenceEquals(null, other) && Equals(Coordinate, other.Coordinate, options);
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        return Coordinate != null ? Coordinate.GetHashCode(options) : 0;
    }

    public static bool operator ==(Point left, Point right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(Point left, Point right)
    {
        return !(left == right);
    }

    #endregion
}
