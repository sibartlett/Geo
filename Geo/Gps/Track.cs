using System.Collections.Generic;
using Geo.Geometries;
using Geo.Gps.Metadata;

namespace Geo.Gps
{
    public class Track
    {
        public Track()
        {
            Metadata = new TrackMetadata();
            Segments = new List<LineString<Fix>>();
        }

        public TrackMetadata Metadata { get; private set; }
        public List<LineString<Fix>> Segments { get; set; }
    }
}