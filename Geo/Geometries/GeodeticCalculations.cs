using Geo.Interfaces;
using Geo.Measure;
using Geo.Reference;

namespace Geo.Geometries
{
    public static class GeodeticCalculations
    {
        public static double CalculateMeridionalParts(this IPosition point)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateMeridionalParts(point.GetCoordinate().Latitude);
        }

        public static Distance CalculateMeridionalDistance(this IPosition point)
        {
            return new Distance(GeoContext.Current.GeodeticCalculator.CalculateMeridionalDistance(point.GetCoordinate().Latitude));
        }

        public static GeodeticLine CalculateShortestLine(this IPosition point1, IPosition point2)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateGreatCircleLine(this IPosition point1, IPosition point2)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateOrthodromicLine(this IPosition point1, IPosition point2)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateRhumbLine(this IPosition point1, IPosition point2)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLoxodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateLoxodromicLine(this IPosition point1, IPosition point2)
        {
            return GeoContext.Current.GeodeticCalculator.CalculateLoxodromicLine(point1, point2);
        }
    }
}
