namespace Geo.Measure;

public enum DistanceUnit
{
    [Unit("m", 1)]
    M = 0,

    [Unit("nm", 1852)]
    Nm = 1,

    [Unit("km", 1000)]
    Km = 2,

    // The international mile is exactly 1609.344 m; the rounded 1609.34 used before
    // left every mile 4 mm short.
    [Unit("mi", 1609.344)]
    Mile = 3,

    [Unit("ft", 0.3048)]
    Ft = 4,
}
