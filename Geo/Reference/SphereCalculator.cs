using System;
using Geo.Geometries;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Reference
{
    public class SphereCalculator : IGeodeticCalculator
    {
        public SphereCalculator() : this(Constants.EarthMeanRadius)
        {
        }

        public SphereCalculator(double radius)
        {
            Radius = radius;
        }

        public double Radius { get; protected set; }


        public GeodeticLine CalculateOrthodromicLine(ILatLng point, double heading, double distance)
        {
            throw new NotImplementedException();
        }

        public GeodeticLine CalculateOrthodromicLine(ILatLng point1, ILatLng point2)
        {
            throw new NotImplementedException();
        }

        public GeodeticLine CalculateLoxodromicLine(ILatLng point1, ILatLng point2)
        {
            throw new NotImplementedException();
        }

        public Area CalculateArea(CoordinateSequence coordinates)
        {
            var area = 0.0;
            if (coordinates.Count > 3 && coordinates.IsClosed)
            {
                for (var i = 0; i < coordinates.Count - 1; i++)
                {
                    var p1 = coordinates[i];
                    var p2 = coordinates[i + 1];
                    area += (p2.Longitude - p1.Longitude).ToRadians() *
                            (2 + Math.Sin(p1.Latitude.ToRadians()) +
                             Math.Sin(p2.Latitude.ToRadians()));
                }
                area = area * Radius * Radius / 2.0;
            }
            return new Area(area);
        }

        public Area CalculateArea(Circle circle)
        {
            if (circle.Radius <= 0)
                return new Area(0d);

            if (circle.Radius > Math.PI * Radius)
                return new Area(0d);

            return new Area(2 * Math.PI * Radius * Radius * (1 - Math.Cos(circle.Radius / Radius)));
        }

        public Area CalculateArea(Envelope envelope)
        {
            var h1 = Radius * (1 - Math.Cos(envelope.MaxLat.ToRadians()));
            var h2 = Radius * (1 - Math.Cos(envelope.MinLat.ToRadians()));
            var zoneArea = 2 * Math.PI * Radius * (h2 - h1);
            var lonPercentage = (envelope.MaxLon - envelope.MinLon) / 360;
            return new Area(zoneArea * lonPercentage);
        }

        public double CalculateMeridionalParts(double latitude)
        {
            throw new NotImplementedException();
        }

        public double CalculateMeridionalDistance(double latitude)
        {
            throw new NotImplementedException();
        }
    }
}