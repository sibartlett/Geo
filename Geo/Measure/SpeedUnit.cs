namespace Geo.Measure;

public enum SpeedUnit
{
    [Unit("m/s", 1)]
    Ms = 0,

    [Unit("knots", 0.514444444)]
    Knots = 1,

    [Unit("kph", 0.277778)]
    Kph = 2,

    [Unit("mph", 0.44704)]
    Mph = 3,
}
