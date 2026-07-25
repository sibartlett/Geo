namespace Geo.Measure;

public enum SpeedUnit
{
    [Unit("m/s", 1)]
    Ms = 0,

    // The factors are written as the exact ratios that define the units rather than as
    // rounded decimals: 0.277778 m/s is not a kilometre per hour, and converting 10 m/s
    // through it gave 35.99997 kph instead of 36.
    [Unit("knots", 1852d / 3600d)]
    Knots = 1,

    [Unit("kph", 1000d / 3600d)]
    Kph = 2,

    [Unit("mph", 0.44704)]
    Mph = 3,
}
