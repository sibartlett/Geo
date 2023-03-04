using System;
using Geo.Geomagnetism;
using NUnit.Framework;

namespace Geo.Tests.Geomagnetism;

[TestFixture]
public class GeomagnetismCalculatorTests
{
    [Test]
    public void Test()
    {
        var a = new IgrfGeomagnetismCalculator();
        var b = a.TryCalculate(new Coordinate(51.8, -0.15), DateTime.UtcNow.Date);
        Console.WriteLine(b);
    }
}