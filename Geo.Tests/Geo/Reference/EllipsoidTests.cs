using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Geometries;
using Geo.Measure;
using Geo.Reference;
using NUnit.Framework;

namespace Geo.Tests.Geo.Reference
{
    [TestFixture]
    public class EllipsoidTests
    {
        [TestCase(25, 1543.0305675812808d)]
        [TestCase(-25, -1543.0305675812808d)]
        public void MeridionalParts(double latitude, double parts)
        {
            var ellipsoid = Ellipsoid.Wgs84();
            var result = ellipsoid.CalculateMeridionalParts(latitude);
            Assert.That(result, Is.EqualTo(parts));
        }

        [TestCase(25, 1493.5497673574687d)]
        public void MeridionalDistance(double latitude, double parts)
        {
            var ellipsoid = Ellipsoid.Wgs84();
            var result = ellipsoid.CalculateMeridionalDistance(latitude);
            Assert.That(result.ConvertTo(DistanceUnit.Nm), Is.EqualTo(parts));
        }

        [TestCase(0, 0, 10, 10, 845.10005801566729d)]
        public void CalculateLoxodromicLineDistance(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var ellipsoid = Ellipsoid.Wgs84();
            var result = ellipsoid.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
            Assert.That(result.Distance.ConvertTo(DistanceUnit.Nm).Value, Is.EqualTo(distance));
        }

        [TestCase(0, 0, 10, 10, 45.04429310980561)]
        public void CalculateLoxodromicCourse(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var ellipsoid = Ellipsoid.Wgs84();
            var result = ellipsoid.CalculateLoxodromicLine(new Point(lat1, lon1), new Point(lat2, lon2));
            Assert.That(result.TrueBearing12, Is.EqualTo(distance));
        }

        [TestCase(0, 0, 10, 10, 45.04429310980561)]
        public void CalculateOrthodromicDestination(double lat1, double lon1, double lat2, double lon2, double distance)
        {
            var ellipsoid = Ellipsoid.Wgs84();
            var result = ellipsoid.CalculateOrthodromicLine(new Point(0, 0), 56, new Distance(34, DistanceUnit.Nm).SiValue);
            Assert.That(result.Coordinate2.Latitude, Is.EqualTo(0.31843626431567573));
            Assert.That(result.Coordinate2.Longitude, Is.EqualTo(0.46895086740918501));
        }
    }
}
