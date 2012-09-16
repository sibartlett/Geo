using Geo.Geometries;
using Geo.Gps.Metadata;

namespace Geo.Gps
{
    public class Route
    {
        public Route()
        {
            Metadata = new RouteMetadata();
            LineString = new LineString();
        }

        public RouteMetadata Metadata { get; private set; }
        public LineString LineString { get; set; }
    }
}