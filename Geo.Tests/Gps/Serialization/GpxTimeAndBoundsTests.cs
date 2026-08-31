using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Geo.Geometries;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

// The file-level <time> and <bounds>, which sit inside <metadata> in GPX 1.1 and
// directly under <gpx> in 1.0. They are handled differently on purpose: the time is
// something only the file can tell us, so it is kept; the bounds only restates the
// extent of the coordinates, so it is computed.
public class GpxTimeAndBoundsTests : SerializerTestFixtureBase
{
    private static readonly DateTime Time = new(2024, 5, 1, 9, 0, 0, DateTimeKind.Utc);

    private static GpsData Parse(string gpx)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = GpsData.Parse(stream);
        Assert.NotNull(data);
        return data!;
    }

    private static GpsData WithAWaypoint()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(53.4808, -2.2426));
        return data;
    }

    [Theory]
    [InlineData("1.1", "http://www.topografix.com/GPX/1/1")]
    [InlineData("1.0", "http://www.topografix.com/GPX/1/0")]
    public void The_file_time_is_read(string version, string ns)
    {
        var inner =
            version == "1.1"
                ? "<metadata><time>2024-05-01T09:00:00Z</time></metadata>"
                : "<time>2024-05-01T09:00:00Z</time>";

        var data = Parse(
            $"<?xml version=\"1.0\"?><gpx version=\"{version}\" xmlns=\"{ns}\">{inner}</gpx>"
        );

        Assert.Equal(Time, data.Metadata.TimeUtc);
        Assert.Equal(DateTimeKind.Utc, data.Metadata.TimeUtc!.Value.Kind);
    }

    [Theory]
    [InlineData(GpxVersion.Gpx11)]
    [InlineData(GpxVersion.Gpx10)]
    public void The_file_time_survives_a_round_trip(GpxVersion version)
    {
        var data = WithAWaypoint();
        data.Metadata.TimeUtc = Time;

        var roundTripped = Parse(data.ToGpx(version));

        Assert.Equal(Time, roundTripped.Metadata.TimeUtc);
    }

    [Theory]
    [InlineData(GpxVersion.Gpx11)]
    [InlineData(GpxVersion.Gpx10)]
    public void A_file_with_no_time_writes_none(GpxVersion version)
    {
        var written = WithAWaypoint().ToGpx(version);

        Assert.DoesNotContain("<time>", written);
        Assert.Null(Parse(written).Metadata.TimeUtc);
    }

    [Fact]
    public void The_file_time_is_written_where_each_schema_sequences_it()
    {
        var data = WithAWaypoint();
        data.Metadata.TimeUtc = Time;
        data.Metadata.Attribute(x => x.Name, "n");
        data.Metadata.Attribute(x => x.Keywords, "k");

        XNamespace ns11 = "http://www.topografix.com/GPX/1/1";
        var metadata = XDocument.Parse(data.ToGpx()).Root!.Element(ns11 + "metadata")!;
        Assert.Equal(
            new[] { "name", "time", "keywords", "bounds" },
            metadata.Elements().Select(x => x.Name.LocalName)
        );

        // 1.0 has no <metadata>; the same fields are children of <gpx>, and the
        // waypoint follows them.
        var root = XDocument.Parse(data.ToGpx(GpxVersion.Gpx10)).Root!;
        Assert.Equal(
            new[] { "name", "time", "keywords", "bounds", "wpt" },
            root.Elements().Select(x => x.Name.LocalName)
        );
    }

    [Fact]
    public void The_bounds_cover_every_kind_of_coordinate()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(10, 20));

        var route = new Route();
        route.Waypoints.Add(new Waypoint(-5, 40));
        data.Routes.Add(route);

        var segment = new TrackSegment();
        segment.Waypoints.Add(new Waypoint(30, -15));
        var track = new Track();
        track.Segments.Add(segment);
        data.Tracks.Add(track);

        Assert.Equal(new Envelope(-5, -15, 30, 40), data.GetBounds());
    }

    [Theory]
    [InlineData(GpxVersion.Gpx11)]
    [InlineData(GpxVersion.Gpx10)]
    public void The_bounds_are_written_from_the_data(GpxVersion version)
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(10, 20));
        data.Waypoints.Add(new Waypoint(-5, 40));

        var written = data.ToGpx(version);
        var bounds = XDocument
            .Parse(written)
            .Descendants()
            .Single(x => x.Name.LocalName == "bounds");

        Assert.Equal("-5", bounds.Attribute("minlat")!.Value);
        Assert.Equal("20", bounds.Attribute("minlon")!.Value);
        Assert.Equal("10", bounds.Attribute("maxlat")!.Value);
        Assert.Equal("40", bounds.Attribute("maxlon")!.Value);
    }

    [Theory]
    [InlineData(GpxVersion.Gpx11)]
    [InlineData(GpxVersion.Gpx10)]
    public void Data_with_no_coordinates_writes_no_bounds(GpxVersion version)
    {
        // An envelope of zeroes would claim the file covers a point in the Atlantic.
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Name, "nothing here");

        var written = data.ToGpx(version);

        Assert.Null(data.GetBounds());
        Assert.DoesNotContain("<bounds", written);
    }

    [Fact]
    public void The_written_bounds_follow_the_data_rather_than_the_file_it_came_from()
    {
        // The point of computing it: a bounds read and kept would still describe the
        // file as it arrived, and would be wrong the moment a caller added a waypoint.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<metadata><bounds minlat=\"10\" minlon=\"20\" maxlat=\"10\" maxlon=\"20\" /></metadata>"
            + "<wpt lat=\"10\" lon=\"20\" />"
            + "</gpx>";

        var data = Parse(gpx);
        data.Waypoints.Add(new Waypoint(50, 60));

        Assert.Equal(new Envelope(10, 20, 50, 60), data.GetBounds());

        XNamespace ns = "http://www.topografix.com/GPX/1/1";
        var written = XDocument.Parse(data.ToGpx()).Descendants(ns + "bounds").Single();

        Assert.Equal("50", written.Attribute("maxlat")!.Value);
        Assert.Equal("60", written.Attribute("maxlon")!.Value);
    }

    [Fact]
    public void An_empty_route_or_segment_does_not_fault_the_bounds()
    {
        // Both have no coordinates to fold in, and Envelope.Combine is an instance
        // method - so a null has to be stepped around rather than passed to it.
        var data = new GpsData();
        data.Routes.Add(new Route());

        var track = new Track();
        track.Segments.Add(new TrackSegment());
        data.Tracks.Add(track);

        Assert.Null(data.GetBounds());

        data.Waypoints.Add(new Waypoint(10, 20));
        Assert.Equal(new Envelope(10, 20, 10, 20), data.GetBounds());
    }

    [Fact]
    public void A_waypoint_with_an_empty_point_does_not_fault_the_bounds()
    {
        // Point.Empty is public and a Waypoint will take one, so it can reach here. It
        // has no coordinate, and this promises null for data with none rather than
        // faulting on it.
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(Point.Empty, DateTime.UtcNow));

        Assert.Null(data.GetBounds());

        data.Waypoints.Add(new Waypoint(10, 20));
        Assert.Equal(new Envelope(10, 20, 10, 20), data.GetBounds());

        var route = new Route();
        route.Waypoints.Add(new Waypoint(Point.Empty, DateTime.UtcNow));
        Assert.Null(route.GetBounds());

        var segment = new TrackSegment();
        segment.Waypoints.Add(new Waypoint(Point.Empty, DateTime.UtcNow));
        Assert.Null(segment.GetBounds());
    }

    [Fact]
    public void The_bounds_of_a_track_route_and_segment_are_their_own()
    {
        var segment = new TrackSegment();
        segment.Waypoints.Add(new Waypoint(10, 20));
        segment.Waypoints.Add(new Waypoint(30, 40));

        var track = new Track();
        track.Segments.Add(segment);

        var route = new Route();
        route.Waypoints.Add(new Waypoint(-5, -10));

        Assert.Equal(new Envelope(10, 20, 30, 40), segment.GetBounds());
        Assert.Equal(new Envelope(10, 20, 30, 40), track.GetBounds());
        Assert.Equal(new Envelope(-5, -10, -5, -10), route.GetBounds());
        Assert.Null(new Route().GetBounds());
        Assert.Null(new TrackSegment().GetBounds());
        Assert.Null(new Track().GetBounds());
    }

    [Fact]
    public void The_reference_files_keep_their_time()
    {
        // 56 of the reference files carry a <time>; this is one of them, and it is
        // read rather than dropped as it used to be.
        var fileInfo = GetReferenceFileDirectory("gpx")
            .EnumerateFiles("cototestmarker.gpx")
            .Single();

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var data = GpsData.Parse(stream);

        Assert.NotNull(data);
        Assert.Equal(new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc), data!.Metadata.TimeUtc);
    }
}
