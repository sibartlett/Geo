#nullable enable
using Geo.Abstractions.Interfaces;

namespace Geo.IO;

/// <summary>
/// The ordinates a geometry has to be written with, taken across every coordinate the
/// writer will visit.
/// </summary>
/// <remarks>
/// <see cref="IGeometry.Is3D" /> and <see cref="IGeometry.IsMeasured" /> are "any
/// coordinate" tests, so a geometry holding both 2D and 3D coordinates reports itself
/// as three-dimensional. A writer that declares the dimensions once for a whole
/// geometry — a WKB type code, a WKT dimension tag — has to write those same ordinates
/// for every coordinate it then emits, or it produces output that cannot be read back.
/// Carrying the two together in one value is what keeps the declaration and the
/// coordinates from drifting apart.
/// </remarks>
internal readonly struct GeometryDimensions
{
    private GeometryDimensions(bool has3D, bool hasMeasure)
    {
        Has3D = has3D;
        HasMeasure = hasMeasure;
    }

    public bool Has3D { get; }
    public bool HasMeasure { get; }

    public static GeometryDimensions For(IGeometry geometry)
    {
        return new GeometryDimensions(geometry.Is3D, geometry.IsMeasured);
    }

    /// <summary>
    /// Drops the ordinates a writer has been configured not to emit.
    /// </summary>
    public GeometryDimensions Limit(int maxDimensions)
    {
        return new GeometryDimensions(Has3D && maxDimensions > 2, HasMeasure && maxDimensions > 3);
    }
}
