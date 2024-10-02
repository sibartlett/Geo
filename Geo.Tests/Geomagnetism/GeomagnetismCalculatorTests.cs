using System;
using Geo.Geomagnetism;
using Xunit;

namespace Geo.Tests.Geomagnetism;

public class GeomagnetismCalculatorTests
{
    [Fact(Skip = "Need to re-visit")]
    public void Test()
    {
        var a = new IgrfGeomagnetismCalculator();
        var b = a.TryCalculate(new Coordinate(51.8, -0.15), DateTime.UtcNow.Date);
        Console.WriteLine(b);
    }
}
