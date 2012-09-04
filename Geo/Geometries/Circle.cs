namespace Geo.Geometries
{
    public class Circle : IGeometry
    {
        protected Circle()
        {
        }

        public Circle(Point center, double radius)
        {
            Center = center;
            Radius = radius;
        }

        public Circle(double latitiude, double longitude, double radius)
        {
            Center = new Point(latitiude, longitude);
            Radius = radius;
        }

        public Point Center { get; protected set; }
        public double Radius { get; protected set; }
    }
}
