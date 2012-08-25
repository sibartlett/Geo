using Geo.Geometries;
using Geo.Measure;
using NUnit.Framework;

namespace Geo.Tests.Geo
{
    [TestFixture]
    public class GeoPointCalculationsTests
    {
        [TestCase(25, 1543.0305675812808d)]
        [TestCase(-25, -1543.0305675812808d)]
        public void MeridionalParts(double latitude, double parts)
        {
            var result = new Point(latitude, 0).CalculateMeridionalParts();
            Assert.That(result, Is.EqualTo(parts));
        }

        [TestCase(25, 1493.5497673574687d)]
        public void MeridionalDistance(double latitude, double parts)
        {
            var result = new Point(latitude, 0).CalculateMeridionalDistance();
            Assert.That(result.ConvertTo(DistanceUnit.Nm).Value, Is.EqualTo(parts));
        }

        [TestCase(0, 0, 10, 10, 845.10005801566729d)]
        public void RhumbLineDistance(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var result = new Point(lat1, lon1).CalculateRhumbLine(new Point(lat2, lon2));
            Assert.That(result.Distance.ConvertTo(DistanceUnit.Nm).Value, Is.EqualTo(distance));
        }

        [TestCase(0, 0, 10, 10, 45.04429310980561)]
        public void RhumbLineCourse(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var result = new Point(lat1, lon1).CalculateRhumbLine(new Point(lat2, lon2));
            Assert.That(result.TrueBearing12, Is.EqualTo(distance));
        }
    }
}
