using Geo.Geometries;
using NUnit.Framework;

namespace Geo.Tests.Geo.Geometries
{
    [TestFixture]
    public class CoordinateTests
    {
        [TestCase("     42.294498        -89.637901         ", 42.294498, -89.637901)]
        [TestCase("12 34.56'N 123 45.55'E", 12.576, 123.75916666666667)]
        [TestCase("12.345°N 123.456°E", 12.345, 123.456)]
        [TestCase("12.345N 123.456E", 12.345, 123.456)]
        [TestCase("12°N 34°W", 12, -34)]
        [TestCase("42.294498, -89.637901", 42.294498, -89.637901)]
        [TestCase("42° 17′ 40″ N, 89° 38′ 16″ W", 42.294444444444444d, -89.637777777777785d)]
        [TestCase("5107212N 00149174W", 51.1202, -1.8195666666666668d)]
        [TestCase("5107212N00149174W", 51.1202, -1.8195666666666668d)]
        public void Parse(string coordinate, double latitude, double longitude)
        {
            var result = Coordinate.Parse(coordinate);
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Latitude, Is.EqualTo(latitude));
            Assert.That(result.Longitude, Is.EqualTo(longitude));
        }
    }
}
