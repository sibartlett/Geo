using Geo.Measure;

namespace Geo.Geometries
{
    public class GeodeticLine
    {
        public GeodeticLine(Coordinate coordinate1, Coordinate coordinate2, double distance, double trueBearing12, double trueBearing21)
        {
            Coordinate1 = coordinate1;
            Coordinate2 = coordinate2;
            TrueBearing12 = trueBearing12;
            TrueBearing21 = trueBearing21;
            Distance = new Distance(distance);
        }

        public Coordinate Coordinate1 { get; private set; }
        public Coordinate Coordinate2 { get; private set; }
        public Distance Distance { get; private set; }
        public double TrueBearing12 { get; private set; }
        public double TrueBearing21 { get; private set; }
    }
}
