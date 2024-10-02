using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Geometries;

public class LineString : Geometry, ICurve
{
    public static readonly LineString Empty = new();

    public LineString()
        : this(new CoordinateSequence()) { }

    public LineString(IEnumerable<Coordinate> coordinates)
        : this(new CoordinateSequence(coordinates)) { }

    public LineString(params Coordinate[] coordinates)
        : this(new CoordinateSequence(coordinates)) { }

    public LineString(CoordinateSequence coordinates)
    {
        Coordinates = coordinates ?? new CoordinateSequence();
    }

    public CoordinateSequence Coordinates { get; }

    public Coordinate this[int index] => Coordinates[index];

    public override Envelope GetBounds()
    {
        return IsEmpty
            ? null
            : new Envelope(
                Coordinates.Min(x => x.Latitude),
                Coordinates.Min(x => x.Longitude),
                Coordinates.Max(x => x.Latitude),
                Coordinates.Max(x => x.Longitude)
            );
    }

    public override bool IsEmpty => Coordinates.IsEmpty;

    public override bool Is3D => Coordinates.HasElevation;

    public override bool IsMeasured => Coordinates.HasM;

    public bool IsClosed => !IsEmpty && Coordinates[0].Equals(Coordinates[Coordinates.Count - 1]);

    public Distance GetLength()
    {
        return GeoContext.Current.GeodeticCalculator.CalculateLength(Coordinates);
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
        var other = obj as LineString;
        return !ReferenceEquals(null, other) && Equals(Coordinates, other.Coordinates, options);
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        return Coordinates != null ? Coordinates.GetHashCode(options) : 0;
    }

    public static bool operator ==(LineString left, LineString right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(LineString left, LineString right)
    {
        return !(left == right);
    }

    #endregion
}
