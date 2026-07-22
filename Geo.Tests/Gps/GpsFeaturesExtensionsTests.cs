using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps;

public class GpsFeaturesExtensionsTests
{
    [Theory]
    [InlineData(GpsFeatures.All, GpsFeatures.Routes, true)]
    [InlineData(GpsFeatures.All, GpsFeatures.Tracks, true)]
    [InlineData(GpsFeatures.All, GpsFeatures.Waypoints, true)]
    [InlineData(GpsFeatures.Routes, GpsFeatures.Tracks, false)]
    [InlineData(GpsFeatures.RoutesAndTracks, GpsFeatures.Waypoints, false)]
    [InlineData(GpsFeatures.RoutesAndTracks, GpsFeatures.Routes, true)]
    [InlineData(GpsFeatures.TracksAndWaypoints, GpsFeatures.Tracks, true)]
    public void Contains_reports_whether_any_requested_flag_is_set(
        GpsFeatures supported,
        GpsFeatures requested,
        bool expected
    )
    {
        Assert.Equal(expected, supported.Contains(requested));
    }

    [Theory]
    [InlineData(GpsFeatures.All, true)]
    [InlineData(GpsFeatures.Routes, true)]
    [InlineData(GpsFeatures.RoutesAndTracks, true)]
    [InlineData(GpsFeatures.Tracks, false)]
    [InlineData(GpsFeatures.Waypoints, false)]
    [InlineData(GpsFeatures.TracksAndWaypoints, false)]
    public void Routes_reflects_the_routes_flag(GpsFeatures supported, bool expected)
    {
        Assert.Equal(expected, supported.Routes());
    }

    [Theory]
    [InlineData(GpsFeatures.All, true)]
    [InlineData(GpsFeatures.Tracks, true)]
    [InlineData(GpsFeatures.RoutesAndTracks, true)]
    [InlineData(GpsFeatures.Routes, false)]
    [InlineData(GpsFeatures.Waypoints, false)]
    [InlineData(GpsFeatures.RoutesAndWaypoints, false)]
    public void Tracks_reflects_the_tracks_flag(GpsFeatures supported, bool expected)
    {
        Assert.Equal(expected, supported.Tracks());
    }

    [Theory]
    [InlineData(GpsFeatures.All, true)]
    [InlineData(GpsFeatures.Waypoints, true)]
    [InlineData(GpsFeatures.RoutesAndWaypoints, true)]
    [InlineData(GpsFeatures.Routes, false)]
    [InlineData(GpsFeatures.Tracks, false)]
    [InlineData(GpsFeatures.RoutesAndTracks, false)]
    public void Waypoints_reflects_the_waypoints_flag(GpsFeatures supported, bool expected)
    {
        Assert.Equal(expected, supported.Waypoints());
    }
}
