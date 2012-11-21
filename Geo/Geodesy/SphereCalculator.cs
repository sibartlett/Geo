using System;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Geodesy
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


        public GeodeticLine CalculateOrthodromicLine(IPosition point, double heading, double distance)
        {
            throw new NotImplementedException();
        }

        public GeodeticLine CalculateOrthodromicLine(IPosition point1, IPosition point2)
        {
            throw new NotImplementedException();
        }

        public GeodeticLine CalculateLoxodromicLine(IPosition point1, IPosition point2)
        {
            throw new NotImplementedException();
        }

        public Distance CalculateLength(Circle circle)
        {
            var h = Radius * (1 - Math.Cos(circle.Radius / Radius));
            var circumference = 2*Math.PI*Math.Sqrt(h*(2*Radius - h));
            return new Distance(circumference);
        }

        public Distance CalculateLength(CoordinateSequence coordinates)
        {
            throw new NotImplementedException();
        }

        public Distance CalculateLength(Envelope envelope)
        {
            var lat = 180 / envelope.MaxLat - envelope.MinLat;
            var lon = 360 / envelope.MaxLon - envelope.MinLon;
            if (envelope.MinLon > envelope.MaxLon)
                lon = 1 - lon;

            var h1 = Radius * (1 - Math.Cos(envelope.MaxLat.ToRadians()));
            var h2 = Radius * (1 - Math.Cos(envelope.MinLat.ToRadians()));

            var r1 = Math.Sqrt(h1*(2*Radius - h1));
            var r2 = Math.Sqrt(h2*(2*Radius - h2));

            var c1 = 2 * Math.PI * r1 * lon;
            var c2 = 2 * Math.PI * r2 * lon;
            var c3 = 2 * Math.PI * Radius * lat * 2;

            return new Distance(c1 + c2 + c3);
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

            var h = Radius * (1 - Math.Cos(circle.Radius / Radius));
            var area = 2 * Math.PI * Radius * h;
            return new Area(area);
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