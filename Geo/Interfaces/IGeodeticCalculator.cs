using System.Collections.Generic;
using Geo.Geometries;
using Geo.Measure;
using Geo.Reference;

namespace Geo.Interfaces
{
    public interface IGeodeticCalculator
    {
        GeodeticLine CalculateOrthodromicLine(IPosition point, double heading, double distance);
        GeodeticLine CalculateOrthodromicLine(IPosition point1, IPosition point2);
        GeodeticLine CalculateLoxodromicLine(IPosition point1, IPosition point2);
        Area CalculateArea(CoordinateSequence coordinates);
        Area CalculateArea(Circle circle);
        Area CalculateArea(Envelope envelope);
        double CalculateMeridionalParts(double latitude);
        double CalculateMeridionalDistance(double latitude);
    }
}