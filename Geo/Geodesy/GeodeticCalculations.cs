using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Geodesy;

public static class GeodeticCalculations
{
    public static double CalculateMeridionalParts(this IPosition point)
    {
        return GeoContext.Current.GeodeticCalculator.CalculateMeridionalParts(
            point.Coordinate.Latitude
        );
    }

    public static Distance CalculateMeridionalDistance(this IPosition point)
    {
        return new Distance(
            GeoContext.Current.GeodeticCalculator.CalculateMeridionalDistance(
                point.Coordinate.Latitude
            )
        );
    }

    public static GeodeticLine CalculateShortestLine(this IPosition point1, IPosition point2)
    {
        return GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(point1, point2);
    }

    public static GeodeticLine CalculateGreatCircleLine(this IPosition point1, IPosition point2)
    {
        return GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(point1, point2);
    }

    public static GeodeticLine CalculateRhumbLine(this IPosition point1, IPosition point2)
    {
        return GeoContext.Current.GeodeticCalculator.CalculateLoxodromicLine(point1, point2);
    }
}
