using System.Collections.Generic;
using System.Linq;
using Geo.Interfaces;
using Geo.Measure;
using Geo.Reference;

namespace Geo.Geometries
{
    public static class GeodeticCalculations
    {
        public static double CalculateMeridionalParts(this ILatLng point)
        {
            return Ellipsoid.Current.CalculateMeridionalParts(point.Latitude);
        }

        public static Distance CalculateMeridionalDistance(this ILatLng point)
        {
            return new Distance(Ellipsoid.Current.CalculateMeridionalDistance(point.Latitude));
        }

        public static GeodeticLine CalculateShortestLine(this ILatLng point1, ILatLng point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateGreatCircleLine(this ILatLng point1, ILatLng point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateOrthodromicLine(this ILatLng point1, ILatLng point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateRhumbLine(this ILatLng point1, ILatLng point2)
        {
            return Ellipsoid.Current.CalculateLoxodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateLoxodromicLine(this ILatLng point1, ILatLng point2)
        {
            return Ellipsoid.Current.CalculateLoxodromicLine(point1, point2);
        }

        public static Distance CalculateShortestDistance<T>(this IEnumerable<T> coordinates) where T : ILatLng
        {
            var distance = new Distance(0);
            var points = coordinates as List<T> ??  coordinates.ToList();
            for (var i = 1; i < points.Count; i++)
            {
                var line = points[i - 1].CalculateShortestLine(points[i]);
                if (line != null)
                    distance += line.Distance;
            }
            return distance;
        }
    }
}
