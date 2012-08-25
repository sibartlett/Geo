using Geo.Measure;
using Geo.Reference;

namespace Geo.Geometries
{
    public static class GeodeticCalculations
    {
        public static double CalculateMeridionalParts(this ILatLngCoordinate point)
        {
            return Ellipsoid.Current.CalculateMeridionalParts(point.Latitude);
        }

        public static Distance CalculateMeridionalDistance(this ILatLngCoordinate point)
        {
            return new Distance(Ellipsoid.Current.CalculateMeridionalDistance(point.Latitude));
        }

        public static GeodeticLine CalculateShortestLine(this ILatLngCoordinate point1, ILatLngCoordinate point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateGreatCircleLine(this ILatLngCoordinate point1, ILatLngCoordinate point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateOrthodromicLine(this ILatLngCoordinate point1, ILatLngCoordinate point2)
        {
            return Ellipsoid.Current.CalculateOrthodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateRhumbLine(this ILatLngCoordinate point1, ILatLngCoordinate point2)
        {
            return Ellipsoid.Current.CalculateLoxodromicLine(point1, point2);
        }

        public static GeodeticLine CalculateLoxodromicLine(this ILatLngCoordinate point1, ILatLngCoordinate point2)
        {
            return Ellipsoid.Current.CalculateLoxodromicLine(point1, point2);
        }
    }
}
