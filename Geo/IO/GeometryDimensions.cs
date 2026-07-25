#nullable enable
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO;

/// <summary>
/// The ordinates a geometry has to be written with, taken across every coordinate the
/// writer will visit.
/// </summary>
/// <remarks>
/// A geometry's own <see cref="IGeometry.Is3D" /> is not enough on its own. It is an
/// "any coordinate" test, so a sequence holding both 2D and 3D coordinates reports
/// itself as three-dimensional, and <see cref="Polygon" /> consults only its shell, so
/// a polygon whose elevations live in a hole reports itself as two-dimensional (and a
/// collection containing one inherits that blind spot). Writers that declare the
/// dimensions once for a whole geometry — a WKB type code, a WKT dimension tag — must
/// agree with the ordinates they then write for every coordinate, or they produce
/// output that cannot be read back.
/// </remarks>
internal readonly struct GeometryDimensions
{
    public static readonly GeometryDimensions None = new(false, false);

    private GeometryDimensions(bool has3D, bool hasMeasure)
    {
        Has3D = has3D;
        HasMeasure = hasMeasure;
    }

    public bool Has3D { get; }
    public bool HasMeasure { get; }

    public static GeometryDimensions For(IGeometry geometry)
    {
        // A polygon's holes are written alongside its shell, so they count towards the
        // dimensions even though Polygon.Is3D/IsMeasured ignore them.
        if (geometry is Polygon polygon)
        {
            if (polygon.IsEmpty)
                return None;

            var dimensions = For(polygon.Shell!);
            foreach (var hole in polygon.Holes)
                dimensions = dimensions.Union(For(hole));
            return dimensions;
        }

        if (geometry is GeometryCollection collection)
        {
            var dimensions = None;
            foreach (var member in collection.Geometries)
                dimensions = dimensions.Union(For(member));
            return dimensions;
        }

        return new GeometryDimensions(geometry.Is3D, geometry.IsMeasured);
    }

    /// <summary>
    /// Drops the ordinates a writer has been configured not to emit.
    /// </summary>
    public GeometryDimensions Limit(int maxDimensions)
    {
        return new GeometryDimensions(Has3D && maxDimensions > 2, HasMeasure && maxDimensions > 3);
    }

    private GeometryDimensions Union(GeometryDimensions other)
    {
        return new GeometryDimensions(Has3D || other.Has3D, HasMeasure || other.HasMeasure);
    }
}
