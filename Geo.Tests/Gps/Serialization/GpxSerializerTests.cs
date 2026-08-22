using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GpxSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void CanParseAll()
    {
        var gpx10 = new Gpx10Serializer();
        var gpx11 = new Gpx11Serializer();
        var dir = GetReferenceFileDirectory("gpx").EnumerateFiles();
        foreach (var fileInfo in dir)
        {
            using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
            var streamWrapper = new StreamWrapper(stream);
            if (gpx10.CanDeSerialize(streamWrapper))
            {
                var data = gpx10.DeSerialize(streamWrapper);
                var gpxData = data.ToGpx();
                Compare(gpx11, data, gpxData);

                gpxData = data.ToGpx(1);
                Compare(gpx10, data, gpxData);
            }
            else if (gpx11.CanDeSerialize(streamWrapper))
            {
                var data = gpx11.DeSerialize(streamWrapper);
                var gpxData = data.ToGpx();
                Compare(gpx11, data, gpxData);

                gpxData = data.ToGpx(1);
                Compare(gpx10, data, gpxData);
            }
            else
            {
                Assert.Fail(fileInfo.Name);
            }
        }
    }

    [Fact]
    public void Gpx_without_namespace_is_parsed_as_gpx11()
    {
        var gpx10 = new Gpx10Serializer();
        var gpx11 = new Gpx11Serializer();
        var fileInfo = GetReferenceFileDirectory("gpx").EnumerateFiles("no-namespace.gpx").Single();

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);

        // A missing default xmlns must not be claimed by the 1.0 serializer...
        Assert.False(gpx10.CanDeSerialize(streamWrapper));
        // ...but should be recognised and parsed by the 1.1 serializer.
        Assert.True(gpx11.CanDeSerialize(streamWrapper));

        var data = gpx11.DeSerialize(streamWrapper);
        Assert.NotNull(data);
        Assert.Equal("runtastic", data.Tracks.Single().Metadata.Attribute(x => x.Name));
        Assert.Equal(3, data.Tracks.Single().Segments.Single().Waypoints.Count);
    }

    [Fact]
    public void Gpx_without_namespace_is_parsed_as_gpx10_when_version_is_1_0()
    {
        var gpx10 = new Gpx10Serializer();
        var gpx11 = new Gpx11Serializer();
        var fileInfo = GetReferenceFileDirectory("gpx")
            .EnumerateFiles("no-namespace-10.gpx")
            .Single();

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);

        // version="1.0" on a namespaceless root routes to the 1.0 serializer...
        Assert.True(gpx10.CanDeSerialize(streamWrapper));
        // ...and not the 1.1 serializer.
        Assert.False(gpx11.CanDeSerialize(streamWrapper));

        var data = gpx10.DeSerialize(streamWrapper);
        Assert.NotNull(data);
        Assert.Equal("legacy track", data.Tracks.Single().Metadata.Attribute(x => x.Name));
        Assert.Equal(3, data.Tracks.Single().Segments.Single().Waypoints.Count);
    }

    [Fact]
    public void Gpx11_serializes_and_round_trips_full_metadata()
    {
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Software, "Geo.Tests");
        data.Metadata.Attribute(x => x.Name, "Sample track");
        data.Metadata.Attribute(x => x.Description, "A sample description");
        data.Metadata.Attribute(x => x.Keywords, "hiking, sample, gpx");
        data.Metadata.Attribute(x => x.Link, "https://example.com/track");
        data.Metadata.Attribute(x => x.Author.Name, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Author.Email, "ada@example.com");
        data.Metadata.Attribute(x => x.Author.Link, "https://example.com/ada");
        data.Metadata.Attribute(x => x.Copyright.Author, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Copyright.License, "CC-BY-4.0");
        data.Metadata.Attribute(x => x.Copyright.Year, "2026");

        var gpx11 = new Gpx11Serializer();
        var gpxData = gpx11.Serialize(data);

        // Every metadata field written above must survive a serialize/deserialize round-trip.
        Assert.Contains("<metadata>", gpxData);
        Compare(gpx11, data, gpxData);
    }

    [Theory]
    [InlineData("nobody")] // no '@' to split on
    [InlineData("@example.com")] // nothing before the '@'
    [InlineData("ada@")] // nothing after it
    public void Gpx11_serializes_an_author_email_it_cannot_split(string email)
    {
        // GPX 1.1 carries an address as a separate id and domain, so one that cannot be
        // split into the two is left out - it must not take the whole document with it.
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Author.Name, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Author.Email, email);

        var gpx = new Gpx11Serializer().Serialize(data);

        Assert.Contains("Ada Lovelace", gpx);
        Assert.DoesNotContain("<email", gpx);
    }

    [Fact]
    public void Gpx11_splits_an_author_email_at_its_first_at_sign()
    {
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Author.Email, "ada@example.com");

        var gpx11 = new Gpx11Serializer();
        var gpx = gpx11.Serialize(data);

        Assert.Contains("id=\"ada\"", gpx);
        Assert.Contains("domain=\"example.com\"", gpx);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var roundTripped = gpx11.DeSerialize(new StreamWrapper(stream));

        Assert.Equal("ada@example.com", roundTripped.Metadata.Attribute(x => x.Author.Email));
    }

    [Fact]
    public void Gpx11_route_without_route_points_is_parsed()
    {
        // <rtept> is optional in the GPX schema, so a route carrying only metadata
        // is valid and must not fault the parser.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" creator=\"Geo.Tests\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<rte><name>empty route</name></rte>"
            + "</gpx>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = new Gpx11Serializer().DeSerialize(new StreamWrapper(stream));

        Assert.NotNull(data);
        Assert.Equal("empty route", data.Routes.Single().Metadata.Attribute(x => x.Name));
        Assert.Empty(data.Routes.Single().Waypoints);
    }

    [Fact]
    public void Gpx10_route_without_route_points_is_parsed()
    {
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" creator=\"Geo.Tests\" xmlns=\"http://www.topografix.com/GPX/1/0\">"
            + "<rte><name>empty route</name></rte>"
            + "</gpx>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = new Gpx10Serializer().DeSerialize(new StreamWrapper(stream));

        Assert.NotNull(data);
        Assert.Equal("empty route", data.Routes.Single().Metadata.Attribute(x => x.Name));
        Assert.Empty(data.Routes.Single().Waypoints);
    }

    [Fact]
    public void Gpx11_writes_a_waypoints_children_in_schema_order()
    {
        // The GPX schema sequences a waypoint's children, and the serializer this
        // replaced emitted its base class's members first, so <link> and <fix> came
        // out after everything else. Written by hand, the order is the schema's.
        var data = new GpsData();
        data.Waypoints.Add(
            new Waypoint(
                new Point(53.4808, -2.2426, 38),
                new DateTime(2024, 5, 1, 9, 0, 0, DateTimeKind.Utc),
                "Manchester",
                "a comment",
                "a description"
            )
        );

        var gpx = XDocument.Parse(new Gpx11Serializer().Serialize(data));
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        var children = gpx.Root!.Element(ns + "wpt")!
            .Elements()
            .Select(x => x.Name.LocalName)
            .ToArray();

        Assert.Equal(new[] { "ele", "time", "name", "cmt", "desc" }, children);
    }

    [Fact]
    public void Gpx11_writes_the_default_namespace_without_a_prefix()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(53.4808, -2.2426));

        var gpx = new Gpx11Serializer().Serialize(data);

        Assert.Contains("xmlns=\"http://www.topografix.com/GPX/1/1\"", gpx);
        Assert.Contains("<wpt ", gpx);
    }

    [Fact]
    public void Gpx10_writes_metadata_as_direct_children_of_gpx()
    {
        // 1.0 has no <metadata> element - the fields 1.1 moved into one are children
        // of the root, in the order the 1.0 schema sequences them.
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Name, "Sample");
        data.Metadata.Attribute(x => x.Description, "A description");
        data.Metadata.Attribute(x => x.Author.Name, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Link, "https://example.com/track");

        var gpx = XDocument.Parse(new Gpx10Serializer().Serialize(data));
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        var children = gpx.Root!.Elements().Select(x => x.Name.LocalName).ToArray();

        Assert.Equal(new[] { "name", "desc", "author", "url" }, children);
        Assert.Null(gpx.Root.Element(ns + "metadata"));
    }

    [Fact]
    public void Gpx11_omits_the_metadata_element_when_there_is_none()
    {
        // Nothing to describe and nothing to take an extent of.
        Assert.DoesNotContain("<metadata", new Gpx11Serializer().Serialize(new GpsData()));
    }

    [Fact]
    public void Gpx11_writes_a_metadata_element_for_bounds_alone()
    {
        // Data with no metadata still has an extent, and <bounds> lives inside
        // <metadata> in 1.1 - so the element appears where it used to be left out.
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(53.4808, -2.2426));

        var gpx = XDocument.Parse(new Gpx11Serializer().Serialize(data));
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        var metadata = gpx.Root!.Element(ns + "metadata");
        Assert.NotNull(metadata);
        Assert.Equal(new[] { "bounds" }, metadata!.Elements().Select(x => x.Name.LocalName));
    }

    [Theory]
    [InlineData("2024-05-01T09:00:00Z", DateTimeKind.Utc)]
    [InlineData("2024-05-01T09:00:00", DateTimeKind.Unspecified)]
    [InlineData("2024-05-01T09:00:00.123Z", DateTimeKind.Utc)]
    public void Gpx11_reads_the_time_formats_the_reference_files_use(
        string time,
        DateTimeKind expected
    )
    {
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<wpt lat=\"53.4808\" lon=\"-2.2426\"><time>"
            + time
            + "</time></wpt>"
            + "</gpx>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = new Gpx11Serializer().DeSerialize(new StreamWrapper(stream));

        var parsed = data.Waypoints.Single().TimeUtc;
        Assert.NotNull(parsed);
        Assert.Equal(expected, parsed!.Value.Kind);
    }

    [Fact]
    public void A_waypoint_with_an_unreadable_elevation_is_still_a_waypoint()
    {
        // The serializer this replaced skipped content it could not bind rather than
        // failing the document, and files in the reference corpus depend on it.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<wpt lat=\"53.4808\" lon=\"-2.2426\"><ele>not a number</ele><name>Manchester</name></wpt>"
            + "</gpx>";

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = new Gpx11Serializer().DeSerialize(new StreamWrapper(stream));

        Assert.NotNull(data);
        var waypoint = data.Waypoints.Single();
        Assert.Equal("Manchester", waypoint.Name);
        Assert.False(waypoint.Coordinate.Is3D);
    }

    [Fact]
    public void DeSerialize_returns_null_for_malformed_xml()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("<gpx"));
        Assert.Null(new Gpx11Serializer().DeSerialize(new StreamWrapper(stream)));
    }

    private void Compare(Gpx10Serializer serializer, GpsData data, string gpxData)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData));
        var data2 = serializer.DeSerialize(new StreamWrapper(stream));
        Compare(data, data2);
    }

    private void Compare(Gpx11Serializer serializer, GpsData data, string gpxData)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData));
        var data2 = serializer.DeSerialize(new StreamWrapper(stream));
        Compare(data, data2);
    }

    // Extension content must survive being written and read back, which is what makes
    // reference/gpx-style.gpx and the Garmin extensions in the corpus worth having.
    // A track segment's extensions are the one exception - GPX 1.0 has nowhere to put
    // them - so they are compared by the tests that know which version was written.
    private static void CompareExtensions(List<XElement> expected, List<XElement> actual)
    {
        Assert.Equal(
            expected.Select(x => x.ToString()).ToArray(),
            actual.Select(x => x.ToString()).ToArray()
        );
    }

    private void Compare(GpsData data, GpsData data2)
    {
        Assert.Equal(data.Metadata.Count, data2.Metadata.Count);
        foreach (var entry in data.Metadata)
            Assert.Equal(entry.Value, data2.Metadata[entry.Key]);

        CompareExtensions(data.Extensions, data2.Extensions);

        // TimeUtc is a property rather than one of the keyed attributes, so the
        // dictionary comparison above does not reach it.
        Assert.Equal(data.Metadata.TimeUtc, data2.Metadata.TimeUtc);

        Assert.Equal(data.Tracks.Count, data2.Tracks.Count);
        for (var i = 0; i < data.Tracks.Count; i++)
        {
            var track1 = data.Tracks[i];
            var track2 = data2.Tracks[i];

            Assert.Equal(track1.Metadata.Count, track2.Metadata.Count);
            foreach (var entry in track1.Metadata)
                Assert.Equal(entry.Value, track2.Metadata[entry.Key]);

            CompareExtensions(track1.Extensions, track2.Extensions);

            Assert.Equal(track1.Segments.Count, track2.Segments.Count);
            for (var s = 0; s < track1.Segments.Count; s++)
            {
                var segment1 = track1.Segments[s];
                var segment2 = track2.Segments[s];

                Assert.Equal(segment1.Waypoints.Count, segment2.Waypoints.Count);
                for (var f = 0; f < segment1.Waypoints.Count; f++)
                {
                    var f1 = segment1.Waypoints[f];
                    var f2 = segment2.Waypoints[f];

                    Compare(f1.Point.Coordinate, f2.Point.Coordinate);
                    Assert.Equal(f1.TimeUtc, f2.TimeUtc);
                    CompareExtensions(f1.Extensions, f2.Extensions);
                }
            }
        }

        Assert.Equal(data.Waypoints.Count, data2.Waypoints.Count);
        for (var i = 0; i < data.Waypoints.Count; i++)
        {
            var wp1 = data.Waypoints[i];
            var wp2 = data2.Waypoints[i];

            Assert.Equal(wp1.Name, wp2.Name);
            Assert.Equal(wp1.Description, wp2.Description);
            Assert.Equal(wp1.Comment, wp2.Comment);
            Compare(wp1.Coordinate, wp2.Coordinate);
            CompareExtensions(wp1.Extensions, wp2.Extensions);
        }

        Assert.Equal(data.Routes.Count, data2.Routes.Count);
        for (var i = 0; i < data.Routes.Count; i++)
        {
            var r1 = data.Routes[i];
            var r2 = data2.Routes[i];

            Assert.Equal(r1.Metadata.Count, r2.Metadata.Count);
            foreach (var entry in r1.Metadata)
                Assert.Equal(entry.Value, r2.Metadata[entry.Key]);

            CompareExtensions(r1.Extensions, r2.Extensions);

            Assert.Equal(r1.Waypoints.Count, r2.Waypoints.Count);
            for (var c = 0; c < r1.Waypoints.Count; c++)
                Compare(r1.Waypoints[c], r2.Waypoints[c]);
        }
    }

    private static void Compare(Waypoint wp1, Waypoint wp2)
    {
        Compare(wp1.Coordinate, wp2.Coordinate);

        Assert.Equal(wp1.Name, wp2.Name);
        Assert.Equal(wp1.Description, wp2.Description);
        Assert.Equal(wp1.Comment, wp2.Comment);

        CompareExtensions(wp1.Extensions, wp2.Extensions);
    }

    private static void Compare(Coordinate coord1, Coordinate coord2)
    {
        Assert.Equal(coord1.Latitude, coord2.Latitude);
        Assert.Equal(coord1.Longitude, coord2.Longitude);

        Assert.Equal(coord1.Is3D, coord2.Is3D);
        if (coord1.Is3D)
            Assert.Equal(((Is3D)coord1).Elevation, ((Is3D)coord2).Elevation);

        Assert.Equal(coord1.IsMeasured, coord2.IsMeasured);
        if (coord1.IsMeasured)
            Assert.Equal(((IsMeasured)coord1).Measure, ((IsMeasured)coord2).Measure);
    }
}
