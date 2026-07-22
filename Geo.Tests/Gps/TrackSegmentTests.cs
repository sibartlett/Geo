using System;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps;

public class TrackSegmentTests
{
    private static Waypoint At(double lat, double lon, int secondsFromEpoch)
    {
        return new Waypoint(
            lat,
            lon,
            0,
            new DateTime(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc).AddSeconds(secondsFromEpoch)
        );
    }

    [Fact]
    public void A_new_segment_is_empty()
    {
        var segment = new TrackSegment();

        Assert.True(segment.IsEmpty());
        Assert.Null(segment.GetFirstWaypoint());
        Assert.Null(segment.GetLastWaypoint());
        Assert.Equal(TimeSpan.Zero, segment.GetDuration());
    }

    [Fact]
    public void First_and_last_waypoints_are_the_endpoints()
    {
        var first = At(0, 0, 0);
        var last = At(0, 3, 30);
        var segment = new TrackSegment();
        segment.Waypoints.Add(first);
        segment.Waypoints.Add(At(0, 1, 10));
        segment.Waypoints.Add(last);

        Assert.False(segment.IsEmpty());
        Assert.Same(first, segment.GetFirstWaypoint());
        Assert.Same(last, segment.GetLastWaypoint());
    }

    [Fact]
    public void ToLineString_carries_every_waypoint_coordinate()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(At(0, 0, 0));
        segment.Waypoints.Add(At(0, 1, 10));

        var lineString = segment.ToLineString();

        Assert.Equal(2, lineString.Coordinates.Count);
    }

    [Fact]
    public void Length_is_positive_and_matches_the_line_string()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(At(0, 0, 0));
        segment.Waypoints.Add(At(0, 1, 10));

        Assert.True(segment.GetLength().SiValue > 0);
        Assert.Equal(segment.ToLineString().GetLength().SiValue, segment.GetLength().SiValue, 1e-6);
    }

    [Fact]
    public void Duration_is_the_span_between_first_and_last_timestamps()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(At(0, 0, 0));
        segment.Waypoints.Add(At(0, 1, 10));
        segment.Waypoints.Add(At(0, 2, 90));

        Assert.Equal(TimeSpan.FromSeconds(90), segment.GetDuration());
    }

    [Fact]
    public void Duration_is_zero_when_timestamps_are_missing()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(new Waypoint(0, 0));
        segment.Waypoints.Add(new Waypoint(0, 1));

        Assert.Equal(TimeSpan.Zero, segment.GetDuration());
    }

    [Fact]
    public void Average_speed_is_length_over_duration()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(At(0, 0, 0));
        segment.Waypoints.Add(At(0, 1, 100));

        var expected = segment.GetLength().SiValue / segment.GetDuration().TotalSeconds;
        Assert.Equal(expected, segment.GetAverageSpeed().SiValue, 1e-6);
    }

    [Fact]
    public void Quantize_thins_waypoints_to_a_minimum_interval()
    {
        var segment = new TrackSegment();
        for (var i = 0; i < 10; i++)
            segment.Waypoints.Add(At(0, i * 0.001, i)); // one waypoint per second

        segment.Quantize(5);

        // Keeps t=0, then the first sample at least 5s later (t=5), then t=10 would be next but
        // the series stops at t=9, so t=0 and t=5 survive.
        Assert.Equal(2, segment.Waypoints.Count);
        Assert.Equal(0, segment.GetFirstWaypoint().TimeUtc.Value.Second);
        Assert.Equal(5, segment.GetLastWaypoint().TimeUtc.Value.Second);
    }

    [Fact]
    public void Quantize_throws_when_a_waypoint_has_no_timestamp()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(At(0, 0, 0));
        segment.Waypoints.Add(new Waypoint(0, 1));

        Assert.Throws<NotSupportedException>(() => segment.Quantize(5));
    }
}
