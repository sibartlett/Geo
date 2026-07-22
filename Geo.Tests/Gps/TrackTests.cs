using System;
using System.Linq;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps;

public class TrackTests
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

    private static TrackSegment Segment(params Waypoint[] waypoints)
    {
        var segment = new TrackSegment();
        segment.Waypoints.AddRange(waypoints);
        return segment;
    }

    [Fact]
    public void A_new_track_is_empty_with_metadata()
    {
        var track = new Track();

        Assert.NotNull(track.Metadata);
        Assert.NotNull(track.Segments);
        Assert.Empty(track.Segments);
        Assert.Null(track.GetFirstSegment());
        Assert.Null(track.GetLastSegment());
        Assert.Null(track.GetFirstWaypoint());
        Assert.Null(track.GetLastWaypoint());
        Assert.Empty(track.GetAllFixes());
    }

    [Fact]
    public void First_and_last_segments_are_the_endpoints()
    {
        var first = Segment(At(0, 0, 0));
        var last = Segment(At(0, 3, 30));
        var track = new Track();
        track.Segments.Add(first);
        track.Segments.Add(Segment(At(0, 1, 10)));
        track.Segments.Add(last);

        Assert.Same(first, track.GetFirstSegment());
        Assert.Same(last, track.GetLastSegment());
    }

    [Fact]
    public void First_and_last_waypoints_span_across_segments()
    {
        var first = At(0, 0, 0);
        var last = At(0, 3, 30);
        var track = new Track();
        track.Segments.Add(Segment(first, At(0, 1, 10)));
        track.Segments.Add(Segment(At(0, 2, 20), last));

        Assert.Same(first, track.GetFirstWaypoint());
        Assert.Same(last, track.GetLastWaypoint());
    }

    [Fact]
    public void GetAllFixes_flattens_every_segment_in_order()
    {
        var track = new Track();
        track.Segments.Add(Segment(At(0, 0, 0), At(0, 1, 10)));
        track.Segments.Add(Segment(At(0, 2, 20)));

        var fixes = track.GetAllFixes().ToList();

        Assert.Equal(3, fixes.Count);
        Assert.Equal(0d, fixes[0].Coordinate.Longitude);
        Assert.Equal(2d, fixes[2].Coordinate.Longitude);
    }

    [Fact]
    public void ToLineString_carries_every_coordinate_across_segments()
    {
        var track = new Track();
        track.Segments.Add(Segment(At(0, 0, 0), At(0, 1, 10)));
        track.Segments.Add(Segment(At(0, 2, 20)));

        var lineString = track.ToLineString();

        Assert.Equal(3, lineString.Coordinates.Count);
    }

    [Fact]
    public void Length_is_positive_and_matches_the_line_string()
    {
        var track = new Track();
        track.Segments.Add(Segment(At(0, 0, 0), At(0, 1, 10)));
        track.Segments.Add(Segment(At(0, 2, 20)));

        Assert.True(track.GetLength().SiValue > 0);
        Assert.Equal(track.ToLineString().GetLength().SiValue, track.GetLength().SiValue, 1e-6);
    }

    [Fact]
    public void Duration_is_the_span_between_first_and_last_timestamps()
    {
        var track = new Track();
        track.Segments.Add(Segment(At(0, 0, 0), At(0, 1, 10)));
        track.Segments.Add(Segment(At(0, 2, 90)));

        Assert.Equal(TimeSpan.FromSeconds(90), track.GetDuration());
    }

    [Fact]
    public void Duration_is_zero_when_timestamps_are_missing()
    {
        var track = new Track();
        track.Segments.Add(Segment(new Waypoint(0, 0), new Waypoint(0, 1)));

        Assert.Equal(TimeSpan.Zero, track.GetDuration());
    }

    [Fact]
    public void Average_speed_is_length_over_duration()
    {
        var track = new Track();
        track.Segments.Add(Segment(At(0, 0, 0), At(0, 1, 100)));

        var expected = track.GetLength().SiValue / track.GetDuration().TotalSeconds;
        Assert.Equal(expected, track.GetAverageSpeed().SiValue, 1e-6);
    }

    [Fact]
    public void Quantize_thins_the_waypoints_of_every_segment()
    {
        var track = new Track();
        var segment = new TrackSegment();
        for (var i = 0; i < 10; i++)
            segment.Waypoints.Add(At(0, i * 0.001, i)); // one waypoint per second
        track.Segments.Add(segment);

        track.Quantize(5);

        // Keeps t=0, then the first sample at least 5s later (t=5); t=10 would be next but the
        // series stops at t=9, so t=0 and t=5 survive.
        Assert.Equal(2, track.Segments[0].Waypoints.Count);
    }
}
