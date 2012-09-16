using Geo.Measure;

namespace Geo.Geometries
{
    public class GeodeticLine
    {
        public GeodeticLine(ICoordinate point1, ICoordinate point2, double distance, double trueBearing12, double trueBearing21)
        {
            Coordinate1 = point1;
            Coordinate2 = point2;
            TrueBearing12 = trueBearing12;
            TrueBearing21 = trueBearing21;
            Distance = new Distance(distance);
        }

        public ICoordinate Coordinate1 { get; private set; }
        public ICoordinate Coordinate2 { get; private set; }
        public Distance Distance { get; private set; }
        public double TrueBearing12 { get; private set; }
        public double TrueBearing21 { get; private set; }
    }
}
