#nullable enable
using System;
using System.Collections.Generic;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Gps;

public class Waypoint : IHasLength
{
    public Waypoint(double latitude, double longitude)
    {
        Point = new Point(latitude, longitude);
    }

    public Waypoint(double latitude, double longitude, double elevation)
    {
        Point = new Point(latitude, longitude, elevation);
    }

    public Waypoint(double latitude, double longitude, double elevation, DateTime dateTime)
    {
        Point = new Point(latitude, longitude, elevation);
        TimeUtc = dateTime;
    }

    public Waypoint(Point point, DateTime dateTime)
    {
        Point = point;
        TimeUtc = dateTime;
    }

    public Waypoint(Point point, string? name, string? comment, string? description)
    {
        Name = name;
        Comment = comment;
        Description = description;
        Point = point;
    }

    public Waypoint(
        Point point,
        DateTime? dateTime,
        string? name,
        string? comment,
        string? description
    )
    {
        Name = name;
        Comment = comment;
        Description = description;
        Point = point;
        TimeUtc = dateTime;
    }

    public string? Name { get; }
    public string? Comment { get; }
    public string? Description { get; }

    public Point Point { get; set; }
    public DateTime? TimeUtc { get; set; }

    public Coordinate Coordinate => Point.Coordinate!;

    /// <summary>
    /// The foreign content carried by this waypoint's GPX element - &lt;wpt&gt;,
    /// &lt;rtept&gt; or &lt;trkpt&gt; - as it appeared.
    /// </summary>
    /// <remarks>
    /// See <see cref="GpsData.Extensions" /> for why this is handed over as XML rather
    /// than modelled. This is where the Garmin &lt;gpxx:WaypointExtension&gt; content
    /// in the reference files ends up. Written back inside &lt;extensions&gt; for GPX
    /// 1.1 and inline for 1.0, which is where each version's schema puts it.
    /// <para>
    /// Only the GPX deserializers populate this; a waypoint read from a format with no
    /// notion of extensions, such as NMEA or IGC, has none.
    /// </para>
    /// </remarks>
    public List<XElement> Extensions { get; } = new List<XElement>();

    public Distance GetLength()
    {
        return ToLineString().GetLength();
    }

    public LineString ToLineString()
    {
        return new LineString(Point.Coordinate!);
    }
}
