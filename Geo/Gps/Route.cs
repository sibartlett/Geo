using System.Collections.Generic;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Measure;

namespace Geo.Gps
{
    public class Route : IHasLength
    {
        public Route()
        {
            Metadata = new RouteMetadata();
            Coordinates = new List<Coordinate>();
        }

        public RouteMetadata Metadata { get; private set; }
        public List<Coordinate> Coordinates { get; set; }

        public LineString ToLineString()
        {
            return new LineString(Coordinates);
        }

        public Distance GetLength()
        {
            return ToLineString().GetLength();
        }
    }
}