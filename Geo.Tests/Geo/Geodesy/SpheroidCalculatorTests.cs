using Geo.Geodesy;
using Geo.Geometries;
using Geo.Measure;
using NUnit.Framework;

namespace Geo.Tests.Geo.Geodesy
{
    [TestFixture]
    public class SpheroidCalculatorTests
    {
        [TestCase(25, 1543.0305675812808d)]
        [TestCase(-25, -1543.0305675812808d)]
        public void MeridionalParts(double latitude, double parts)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateMeridionalParts(latitude);
            Assert.That(result, Is.EqualTo(parts));
        }

        [TestCase(25, 1493.5497673574687d)]
        public void MeridionalDistance(double latitude, double parts)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateMeridionalDistance(latitude);
            Assert.That(result.ConvertTo(DistanceUnit.Nm), Is.EqualTo(parts));
        }

        [TestCase(0, 0, 10, 10, 845.10005801566729d)]
        public void CalculateLoxodromicLineDistance(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
            Assert.That(result.Distance.ConvertTo(DistanceUnit.Nm).Value, Is.EqualTo(distance));
        }

        [TestCase(0, 0, 10, 10, 45.04429310980561)]
        public void CalculateLoxodromicCourse(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
            Assert.That(result.Bearing12, Is.EqualTo(distance));
        }

        [TestCase(0, 0, 10, 10, 44.751910170513703d, 225.62903685894344d)]
        public void CalculateOrthodromicCourse(double lat1, double lon1, double lat2, double lon2, double c12, double c21)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateOrthodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
            Assert.That(result.Bearing12, Is.EqualTo(c12));
            Assert.That(result.Bearing21, Is.EqualTo(c21));
        }

        [TestCase(0, 0, 56, 34, 0.31843626431567573, 0.46895086740918501)]
        [TestCase(-9.443333, 147.216667, 327.91252158264797, 50, -8.7337170355022362, 146.76964429447122d)]
        public void CalculateOrthodromicDestination(double lat1, double lon1, double angle, double distance, double lat2, double lon2)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateOrthodromicLine(new Point(lat1, lon1), angle, new Distance(distance, DistanceUnit.Nm).SiValue);
            Assert.That(result.Coordinate2.Latitude, Is.EqualTo(lat2));
            Assert.That(result.Coordinate2.Longitude, Is.EqualTo(lon2));
        }
    }
}
