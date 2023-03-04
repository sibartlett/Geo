using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Geometries;

public class Polygon : Geometry, ISurface
{
    public static readonly Polygon Empty = new();

    public Polygon() : this((LinearRing)null)
    {
    }

    public Polygon(LinearRing shell, IEnumerable<LinearRing> holes)
    {
        Shell = shell;
        Holes = new SpatialReadOnlyCollection<LinearRing>(holes ?? new LinearRing[0]);
    }

    public Polygon(LinearRing shell, params LinearRing[] holes) : this(shell, (IEnumerable<LinearRing>)holes)
    {
    }

    public Polygon(params Coordinate[] shell) : this(new LinearRing(shell))
    {
    }

    public Polygon(IEnumerable<Coordinate> shell) : this(new LinearRing(shell))
    {
    }

    public LinearRing Shell { get; }
    public SpatialReadOnlyCollection<LinearRing> Holes { get; }

    public override bool IsEmpty => Shell == null || Shell.IsEmpty;

    public override bool Is3D => !IsEmpty && Shell.Is3D;

    public override bool IsMeasured => !IsEmpty && Shell.IsMeasured;

    public override Envelope GetBounds()
    {
        return Shell.GetBounds();
    }

    public Area GetArea()
    {
        var calculator = GeoContext.Current.GeodeticCalculator;
        var area = calculator.CalculateArea(Shell.Coordinates);
        return Holes.Aggregate(area, (current, hole) => current - calculator.CalculateArea(hole.Coordinates));
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
        var other = obj as Polygon;

        if (ReferenceEquals(null, other))
            return false;

        if (IsEmpty && other.IsEmpty)
            return true;

        return Shell.Equals(other.Shell, options)
               && Holes.Count == other.Holes.Count
               && !Holes
                   .Where((t, i) => !t.Equals(other.Holes[i], options))
                   .Any();
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            return ((Shell != null ? Shell.GetHashCode(options) : 0) * 397) ^
                   (Holes != null ? Holes.GetHashCode(options) : 0);
        }
    }

    public static bool operator ==(Polygon left, Polygon right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(Polygon left, Polygon right)
    {
        return !(left == right);
    }

    #endregion
}