using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Measure;

namespace Geo.Gps;

public class Route : IHasLength
{
    public Route()
    {
        Metadata = new RouteMetadata();
        Waypoints = new List<Waypoint>();
    }

    public RouteMetadata Metadata { get; }
    public List<Waypoint> Waypoints { get; set; }

    public Distance GetLength()
    {
        return ToLineString().GetLength();
    }

    public LineString ToLineString()
    {
        return new LineString(Waypoints.Select(wp => wp.Coordinate));
    }
}
