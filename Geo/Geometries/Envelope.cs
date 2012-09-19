using System;

namespace Geo.Geometries
{
    public class Envelope : IGeometry, IWktShape
    {
        public Envelope(double minLat, double minLon, double maxLat, double maxLon)
        {
            MinLat = minLat;
            MinLon = minLon;
            MaxLat = maxLat;
            MaxLon = maxLon;
        }

        public double MinLat { get; private set; }
        public double MinLon { get; private set; }
        public double MaxLat { get; private set; }
        public double MaxLon { get; private set; }

        public Envelope CreateNewEnvelope(Envelope other)
        {
            if (other == null)
            {
                return new Envelope(MinLat, MinLon, MaxLat, MaxLon);
            }

            return new Envelope(
                Math.Min(MinLat, other.MinLat),
                Math.Min(MinLon, other.MinLon),
                Math.Min(MaxLat, other.MaxLat),
                Math.Min(MaxLon, other.MaxLon)
            );
        }

        public Envelope GetBounds()
        {
            return this;
        }

        public Polygon ToPolygon()
        {
            return new Polygon(
                new LinearRing(new [] {
                    new Coordinate(MinLat, MinLon), 
                    new Coordinate(MaxLat, MinLon), 
                    new Coordinate(MaxLat, MaxLon), 
                    new Coordinate(MinLat, MaxLon),
                    new Coordinate(MinLat, MinLon),
                })
            );
        }

        public string ToWktString()
        {
            return ToPolygon().ToWktString();
        }
    }
}
