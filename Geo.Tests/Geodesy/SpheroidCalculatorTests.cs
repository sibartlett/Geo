using Geo.Geodesy;
using Geo.Geometries;
using Geo.Measure;
using NUnit.Framework;

namespace Geo.Tests.Geodesy
{
    [TestFixture]
    public class SpheroidCalculatorTests
    {
		private const double Millionth = 0.000001;

        [TestCase(25, 1543.030567)]
        [TestCase(-25, -1543.030567)]
        public void MeridionalParts(double latitude, double parts)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateMeridionalParts(latitude);
            Assert.That(result, Is.EqualTo(parts).Within(Millionth));
        }

        [TestCase(25, 1493.549767)]
        public void MeridionalDistance(double latitude, double parts)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateMeridionalDistance(latitude);
            Assert.That(result.ConvertTo(DistanceUnit.Nm), Is.EqualTo(parts).Within(Millionth));
        }

        [TestCase(0, 0, 10, 10, 845.100058)]
        public void CalculateLoxodromicLineDistance(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
			Assert.That(result.Distance.ConvertTo(DistanceUnit.Nm).Value, Is.EqualTo(distance).Within(Millionth));
        }

        [TestCase(0, 0, 10, 10, 45.044293)]
        public void CalculateLoxodromicCourse(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
			Assert.That(result.Bearing12, Is.EqualTo(distance).Within(Millionth));
        }

        [TestCase(0, 0, 10, 10, 44.751910, 225.629037)]
        public void CalculateOrthodromicCourse(double lat1, double lon1, double lat2, double lon2, double c12, double c21)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateOrthodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
			Assert.That(result.Bearing12, Is.EqualTo(c12).Within(Millionth));
			Assert.That(result.Bearing21, Is.EqualTo(c21).Within(Millionth));
        }

        [TestCase(0, 0, 56, 34, 0.318436, 0.468951)]
        [TestCase(-9.443333, 147.216667, 327.912522, 50, -8.733717, 146.769644)]
        public void CalculateOrthodromicDestination(double lat1, double lon1, double angle, double distance, double lat2, double lon2)
        {
            var calculator = SpheroidCalculator.Wgs84();
            var result = calculator.CalculateOrthodromicLine(new Point(lat1, lon1), angle, new Distance(distance, DistanceUnit.Nm).SiValue);
			Assert.That(result.Coordinate2.Latitude, Is.EqualTo(lat2).Within(Millionth));
			Assert.That(result.Coordinate2.Longitude, Is.EqualTo(lon2).Within(Millionth));
        }
    }
}
