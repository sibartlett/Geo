namespace Geo.Gps;

/// <summary>
/// A version of the GPX format.
/// </summary>
/// <remarks>
/// The members are numbered after the versions they name rather than left to default
/// from zero. C# lets the literal <c>0</c> convert to any enum whatever its members,
/// so a caller who wrote <c>ToGpx(0)</c> against the old decimal overload - which
/// meant GPX 1.1 - would otherwise still compile and quietly get 1.0 instead. With no
/// member at zero it reaches <see cref="GpsData.ToGpx(GpxVersion)" /> as an undefined
/// value and is rejected, which is the kind of change that should be noticed.
/// </remarks>
public enum GpxVersion
{
    /// <summary>
    /// GPX 1.0.
    /// </summary>
    Gpx10 = 10,

    /// <summary>
    /// GPX 1.1, the current version of the format and the one written by default.
    /// </summary>
    Gpx11 = 11,
}
