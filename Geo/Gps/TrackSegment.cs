using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Gps;

public class TrackSegment : IHasLength
{
    public TrackSegment()
    {
        Waypoints = new List<Waypoint>();
    }

    public List<Waypoint> Waypoints { get; set; }

    /// <summary>
    /// The foreign content carried by this segment's &lt;trkseg&gt; element, as it
    /// appeared.
    /// </summary>
    /// <remarks>
    /// See <see cref="GpsData.Extensions" /> for why this is handed over as XML rather
    /// than modelled.
    /// <para>
    /// This is the one place GPX 1.0 has no room for: its schema ends &lt;trkseg&gt;
    /// with &lt;trkpt&gt; and admits no foreign element after it. A segment's
    /// extensions are therefore read and written for 1.1 only, and writing a 1.0
    /// document drops them.
    /// </para>
    /// </remarks>
    public List<XElement> Extensions { get; } = new List<XElement>();

    public Distance GetLength()
    {
        return ToLineString().GetLength();
    }

    public LineString ToLineString()
    {
        return new LineString(Waypoints.Select(x => x.Coordinate));
    }

    /// <summary>
    /// The smallest envelope containing every coordinate in this segment, or <c>null</c> when there
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
        return Waypoints.Aggregate(
            (Envelope?)null,
            // Point.GetBounds is null-safe where Waypoint.Coordinate is not: a waypoint
            // holding an empty Point has no coordinate, and this promises null for data
            // with none rather than faulting on it.
            (bounds, waypoint) => waypoint.Point.GetBounds()?.Combine(bounds) ?? bounds
        );
    }

    public bool IsEmpty()
    {
        return Waypoints.Count == 0;
    }

    public Waypoint? GetFirstWaypoint()
    {
        return IsEmpty() ? default : Waypoints[0];
    }

    public Waypoint? GetLastWaypoint()
    {
        return IsEmpty() ? default : Waypoints[Waypoints.Count - 1];
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
        if (Waypoints.Any(x => !x.TimeUtc.HasValue))
            throw new NotSupportedException(
                "All waypoints require a timestamp, for track segment to be quantized."
            );

        var waypoints = new List<Waypoint>();
        Waypoint? lastWaypoint = null;
        foreach (var waypoint in Waypoints)
            if (
                lastWaypoint == null
                || Math.Abs((waypoint.TimeUtc!.Value - lastWaypoint.TimeUtc!.Value).TotalSeconds)
                    >= seconds
            )
            {
                lastWaypoint = waypoint;
                waypoints.Add(waypoint);
            }

        Waypoints = waypoints;
    }
}
