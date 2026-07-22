using System;
using System.IO;
using System.Linq;
using System.Text;
using Geo.Geometries;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GpsDataTests : SerializerTestFixtureBase
{
    private static GpsData SampleData()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(new Point(51.5, -0.12), "Home", "comment", "description"));
        return data;
    }

    [Theory]
    [InlineData("gpx", "Bergamo to Manchester.gpx")]
    [InlineData("igc", "igc2.igc")]
    [InlineData("nmea", "Stockholm_Walk.nmea")]
    [InlineData("garmin", "garmin.fpl")]
    [InlineData("skydemon", "skydemon.flightplan")]
    [InlineData("pocketfms", "pocketfms_fp.xml")]
    public void Parse_detects_the_format_and_returns_data(string subDirectory, string fileName)
    {
        var file = GetReferenceFileDirectory(subDirectory)
            .GetFiles()
            .First(x => x.Name == fileName);
        using var stream = file.OpenRead();

        var data = GpsData.Parse(stream);

        Assert.NotNull(data);
    }

    [Fact]
    public void Parse_routes_an_igc_file_to_a_track()
    {
        var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
        using var stream = file.OpenRead();

        var data = GpsData.Parse(stream);

        Assert.NotEmpty(data.Tracks);
        Assert.Empty(data.Routes);
    }

    [Fact]
    public void Parse_routes_a_flightplan_to_a_route()
    {
        var file = GetReferenceFileDirectory("garmin")
            .GetFiles()
            .First(x => x.Name == "garmin.fpl");
        using var stream = file.OpenRead();

        var data = GpsData.Parse(stream);

        Assert.NotEmpty(data.Routes);
        Assert.Empty(data.Tracks);
    }

    [Fact]
    public void Parse_returns_null_for_unrecognised_content()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is not a gps file"));

        Assert.Null(GpsData.Parse(stream));
    }

    [Fact]
    public void ToGpx_defaults_to_version_1_1()
    {
        Assert.Contains("version=\"1.1\"", SampleData().ToGpx());
    }

    [Fact]
    public void ToGpx_with_version_1_emits_gpx_1_0()
    {
        Assert.Contains("version=\"1.0\"", SampleData().ToGpx(1m));
    }

    [Fact]
    public void ToGpx_with_version_1_1_emits_gpx_1_1()
    {
        Assert.Contains("version=\"1.1\"", SampleData().ToGpx(1.1m));
    }

    [Fact]
    public void ToGpx_round_trips_through_parse()
    {
        var gpx = SampleData().ToGpx();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var parsed = GpsData.Parse(stream);

        var waypoint = Assert.Single(parsed.Waypoints);
        Assert.Equal(51.5, waypoint.Coordinate.Latitude);
        Assert.Equal(-0.12, waypoint.Coordinate.Longitude);
        Assert.Equal("Home", waypoint.Name);
    }

    [Fact]
    public void SupportedGpsFileFormats_lists_every_supported_format()
    {
        var names = GpsData.SupportedGpsFileFormats().Select(x => x.Name).ToList();

        Assert.Contains("GPX 1.0", names);
        Assert.Contains("GPX 1.1", names);
        Assert.Contains("IGC", names);
        Assert.Contains("NMEA", names);
        Assert.Contains("Garmin Flightplan", names);
        Assert.Contains("SkyDemon Flightplan", names);
        Assert.Contains("PocketFMS Flightplan", names);
    }

    [Fact]
    public void SupportedGpsFileFormats_are_ordered_by_extension_then_name()
    {
        var formats = GpsData.SupportedGpsFileFormats().ToList();

        var expected = formats.OrderBy(x => x.Extension).ThenBy(x => x.Name).Select(x => x.Name);
        Assert.Equal(expected, formats.Select(x => x.Name));
    }

    [Fact]
    public void SupportedGpsFileFormats_for_routes_excludes_track_only_formats()
    {
        var names = GpsData
            .SupportedGpsFileFormats(GpsFeatures.Routes)
            .Select(x => x.Name)
            .ToList();

        Assert.Contains("Garmin Flightplan", names);
        Assert.Contains("SkyDemon Flightplan", names);
        Assert.Contains("PocketFMS Flightplan", names);
        Assert.Contains("GPX 1.1", names); // GPX supports every feature
        Assert.DoesNotContain("IGC", names); // Tracks only
        Assert.DoesNotContain("NMEA", names); // Tracks and Waypoints only
    }

    [Fact]
    public void SupportedGpsFileFormats_for_tracks_includes_igc_and_nmea()
    {
        var names = GpsData
            .SupportedGpsFileFormats(GpsFeatures.Tracks)
            .Select(x => x.Name)
            .ToList();

        Assert.Contains("IGC", names);
        Assert.Contains("NMEA", names);
        Assert.Contains("GPX 1.0", names);
        Assert.DoesNotContain("Garmin Flightplan", names);
        Assert.DoesNotContain("SkyDemon Flightplan", names);
        Assert.DoesNotContain("PocketFMS Flightplan", names);
    }

    [Fact]
    public void SupportedGpsFileFormats_for_waypoints_includes_nmea_but_not_igc()
    {
        var names = GpsData
            .SupportedGpsFileFormats(GpsFeatures.Waypoints)
            .Select(x => x.Name)
            .ToList();

        Assert.Contains("NMEA", names);
        Assert.Contains("GPX 1.1", names);
        Assert.DoesNotContain("IGC", names); // Tracks only
        Assert.DoesNotContain("Garmin Flightplan", names); // Routes only
    }
}
