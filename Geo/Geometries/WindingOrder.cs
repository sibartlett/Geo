#nullable enable
namespace Geo.Geometries;

/// <summary>
/// The direction in which the vertices of a ring are ordered. By convention a
/// polygon's exterior ring (shell) is <see cref="CounterClockwise" /> and its interior
/// rings (holes) are <see cref="Clockwise" />.
/// </summary>
public enum WindingOrder
{
    /// <summary>The vertices are ordered clockwise.</summary>
    Clockwise,

    /// <summary>The vertices are ordered counter-clockwise.</summary>
    CounterClockwise,
}
