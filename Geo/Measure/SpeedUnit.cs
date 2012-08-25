namespace Geo.Measure
{
    public enum SpeedUnit
    {
        [Unit("m/s", 1)]
        Ms = 0,
        [Unit("knots", 1.9438)]
        Knots = 1,
        [Unit("kph", 3.6)]
        Kph = 2,
        [Unit("mph", 2.2369)]
        Mph = 3
    }
}