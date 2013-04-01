using System;
using Geo.Geodesy;
using Geo.Geomagnetism;
using NUnit.Framework;

namespace Geo.Tests.Geomagnetism
{
    [TestFixture]
    public class GeomagnetismCalculatorTests
    {
        [Test]
        public void Test()
        {
            var a = new GeomagnetismCalculator(Spheroid.Wgs84);
            var b = a.TryCalculate(new Coordinate(51.8, -0.15), DateTime.UtcNow.Date, GeomagnetismSource.Igrf);
            Console.WriteLine(b);
        }
    }
}
