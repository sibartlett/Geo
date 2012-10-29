using System;
using System.Collections.Generic;
using System.Linq;
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

        public Area CalculateArea(IEnumerable<Coordinate> coordinates)
        {
            var points = coordinates as List<Coordinate> ?? coordinates.ToList();
            var area = 0.0;
            if (points.Count > 3 && points[0] == points[points.Count - 1])
            {
                for (var i = 0; i < points.Count - 1; i++)
                {
                    var p1 = points[i];
                    var p2 = points[i + 1];
                    area += (p2.Longitude - p1.Longitude).ToRadians() *
                            (2 + Math.Sin(p1.Latitude.ToRadians()) +
                             Math.Sin(p2.Latitude.ToRadians()));
                }
                area = area * Radius * Radius / 2.0;
            }
            return new Area(area);
        }

        public Area CalculateArea(ILatLng point, double radius)
        {
            if (radius <= 0)
                return new Area(0d);

            if (radius > Math.PI * Radius)
                return new Area(0d);

            return new Area(2 * Math.PI * Radius * Radius * (1 - Math.Cos(radius / Radius)));
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