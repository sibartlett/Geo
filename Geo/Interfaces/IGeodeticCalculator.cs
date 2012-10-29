using System.Collections.Generic;
using Geo.Geometries;
using Geo.Measure;
using Geo.Reference;

namespace Geo.Interfaces
{
    public interface IGeodeticCalculator
    {
        GeodeticLine CalculateOrthodromicLine(ILatLng point, double heading, double distance);
        GeodeticLine CalculateOrthodromicLine(ILatLng point1, ILatLng point2);
        GeodeticLine CalculateLoxodromicLine(ILatLng point1, ILatLng point2);
        Area CalculateArea(IEnumerable<Coordinate> coordinates);
        Area CalculateArea(ILatLng point, double radius);
        double CalculateMeridionalParts(double latitude);
        double CalculateMeridionalDistance(double latitude);
    }
}