namespace Geo.Geometries
{
    public class Circle : IGeometry
    {
        protected Circle()
        {
        }

        public Circle(Coordinate center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public Circle(double latitiude, double longitude, double radius)
        {
            Center = new Coordinate(latitiude, longitude);
            Radius = radius;
        }

        public Coordinate Center { get; protected set; }
        public double Radius { get; protected set; }
    }
}
