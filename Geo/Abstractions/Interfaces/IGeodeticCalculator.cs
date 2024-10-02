using Geo.Geodesy;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Abstractions.Interfaces;

public interface IGeodeticCalculator
{
    GeodeticLine CalculateOrthodromicLine(IPosition point, double heading, double distance);
    GeodeticLine CalculateOrthodromicLine(IPosition point1, IPosition point2);
    GeodeticLine CalculateLoxodromicLine(IPosition point1, IPosition point2);

    Distance CalculateLength(Circle circle);
    Distance CalculateLength(CoordinateSequence coordinates);
    Distance CalculateLength(Envelope envelope);

    Area CalculateArea(Circle circle);
    Area CalculateArea(CoordinateSequence coordinates);
    Area CalculateArea(Envelope envelope);

    double CalculateMeridionalParts(double latitude);
    double CalculateMeridionalDistance(double latitude);
}
