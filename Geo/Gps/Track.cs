using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Measure;

namespace Geo.Gps;

public class Track : IHasLength
{
    public Track()
    {
        Metadata = new TrackMetadata();
        Segments = new List<TrackSegment>();
    }

    public TrackMetadata Metadata { get; }
    public List<TrackSegment> Segments { get; set; }

    /// <summary>
    /// The foreign content carried by this track's GPX element, as it appeared.
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
        return new LineString(Segments.SelectMany(x => x.Waypoints).Select(x => x.Coordinate));
    }

    /// <summary>
    /// The smallest envelope containing every coordinate in this track, or <c>null</c> when there
    /// are none.
    /// </summary>
    /// <remarks>
    /// GPX defines its &lt;bounds&gt; element as the extent of the coordinates in the
    /// file, so the serializers compute it from the data at the point of writing rather
    /// than storing what they read. Kept, it would go stale the moment a caller added a
    /// waypoint, and the file would then carry an extent that did not describe it.
    /// </remarks>
    public Envelope? GetBounds()
    {
        return Segments.Aggregate(
            (Envelope?)null,
            // A segment with no waypoints has no bounds to fold in; Combine is an
            // instance method, so the null has to be stepped around rather than passed.
            (bounds, segment) => segment.GetBounds()?.Combine(bounds) ?? bounds
        );
    }

    public TrackSegment? GetFirstSegment()
    {
        return Segments.Count == 0 ? default : Segments[0];
    }

    public TrackSegment? GetLastSegment()
    {
        return Segments.Count == 0 ? default : Segments[Segments.Count - 1];
    }

    public IEnumerable<Waypoint> GetAllFixes()
    {
        return Segments.SelectMany(x => x.Waypoints);
    }

    public Waypoint? GetFirstWaypoint()
    {
        var segment = GetFirstSegment();
        return segment == null ? default : segment.GetFirstWaypoint();
    }

    public Waypoint? GetLastWaypoint()
    {
        var segment = GetLastSegment();
        return segment == null ? default : segment.GetLastWaypoint();
    }

    public Speed GetAverageSpeed()
    {
        return new Speed(GetLength().SiValue, GetDuration());
    }

    public TimeSpan GetDuration()
    {
        var first = GetFirstWaypoint();
        var last = GetLastWaypoint();
        if (first?.TimeUtc != null && last?.TimeUtc != null)
            return last.TimeUtc.Value - first.TimeUtc.Value;
        return TimeSpan.Zero;
    }

    public void Quantize(double seconds = 0)
    {
        foreach (var segment in Segments)
            segment.Quantize(seconds);
    }
}
