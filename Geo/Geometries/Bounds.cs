using System;

namespace Geo.Geometries
{
    public class Bounds
    {
        public Bounds(double minLat, double minLon, double maxLat, double maxLon)
        {
            MinLat = minLat;
            MinLon = minLon;
            MaxLat = maxLat;
            MaxLon = maxLon;
        }

        public double MinLat { get; set; }
        public double MinLon { get; set; }
        public double MaxLat { get; set; }
        public double MaxLon { get; set; }

        public Bounds CreateNewMaxBounds(Bounds other)
        {
            if (other == null)
            {
                return new Bounds(MinLat, MinLon, MaxLat, MaxLon);
            }

            return new Bounds(
                Math.Min(MinLat, other.MinLat),
                Math.Min(MinLon, other.MinLon),
                Math.Min(MaxLat, other.MaxLat),
                Math.Min(MaxLon, other.MaxLon)
            );
        }
    }
}
