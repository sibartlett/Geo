using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
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

    /// <summary>
    /// The foreign content carried by this route's GPX element, as it appeared.
    /// </summary>
    /// <remarks>
    /// See <see cref="GpsData.Extensions" /> for why this is handed over as XML rather
    /// than modelled. Written back inside &lt;extensions&gt; for GPX 1.1 and inline for
    /// 1.0, which is where each version's schema puts it.
    /// </remarks>
    public List<XElement> Extensions { get; } = new List<XElement>();

    public Distance GetLength()
    {
        return ToLineString().GetLength();
    }

    public LineString ToLineString()
    {
        return new LineString(Waypoints.Select(wp => wp.Coordinate));
    }
}
