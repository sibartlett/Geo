using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

// GPX leaves <extensions> open, so the library carries the content through rather
// than modelling it. These cover that it is read from where each version puts it,
// written back to where each version puts it, and not quietly altered on the way.
public class GpxExtensionsTests : SerializerTestFixtureBase
{
    private static readonly XNamespace Style = "http://www.topografix.com/GPX/gpx_style/0/2";
    private static readonly XNamespace Garmin = "http://www.garmin.com/xmlschemas/GpxExtensions/v3";

    private const string Gpx11WithExtensions =
        "<?xml version=\"1.0\"?>"
        + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\""
        + " xmlns:gpx_style=\"http://www.topografix.com/GPX/gpx_style/0/2\">"
        + "<metadata><name>Styled</name>"
        + "<extensions><gpx_style:meta>m</gpx_style:meta></extensions>"
        + "</metadata>"
        + "<wpt lat=\"1\" lon=\"2\">"
        + "<extensions><gpx_style:line><gpx_style:color>1B7F3B</gpx_style:color></gpx_style:line></extensions>"
        + "</wpt>"
        + "<rte><name>r</name>"
        + "<extensions><gpx_style:line><gpx_style:color>0000FF</gpx_style:color></gpx_style:line></extensions>"
        + "<rtept lat=\"3\" lon=\"4\" /></rte>"
        + "<trk><name>t</name>"
        + "<extensions><gpx_style:line><gpx_style:color>C00000</gpx_style:color></gpx_style:line></extensions>"
        + "<trkseg><trkpt lat=\"5\" lon=\"6\" />"
        + "<extensions><gpx_style:seg>s</gpx_style:seg></extensions>"
        + "</trkseg></trk>"
        + "<extensions><gpx_style:file>f</gpx_style:file></extensions>"
        + "</gpx>";

    private static GpsData Parse(string gpx)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = GpsData.Parse(stream);
        Assert.NotNull(data);
        return data!;
    }

    [Fact]
    public void Gpx11_extensions_are_read_from_every_element_that_can_hold_them()
    {
        var data = Parse(Gpx11WithExtensions);

        // The <gpx> and <metadata> extensions both land on GpsData - there is no
        // domain object for the metadata element, and GPX 1.0 has no such element.
        Assert.Equal(
            new[] { "file", "meta" },
            data.Extensions.Select(x => x.Name.LocalName).OrderBy(x => x)
        );

        Assert.Equal(Style + "line", data.Waypoints.Single().Extensions.Single().Name);
        Assert.Equal(Style + "line", data.Routes.Single().Extensions.Single().Name);
        Assert.Equal(Style + "line", data.Tracks.Single().Extensions.Single().Name);
        Assert.Equal(
            Style + "seg",
            data.Tracks.Single().Segments.Single().Extensions.Single().Name
        );
    }

    [Fact]
    public void An_extensions_content_is_read_whole()
    {
        var line = Parse(Gpx11WithExtensions).Tracks.Single().Extensions.Single();

        Assert.Equal("C00000", line.Element(Style + "color")!.Value);
    }

    [Fact]
    public void Gpx11_extensions_survive_a_round_trip()
    {
        var data = Parse(Gpx11WithExtensions);
        var roundTripped = Parse(data.ToGpx());

        Assert.Equal(
            new[] { "file", "meta" },
            roundTripped.Extensions.Select(x => x.Name.LocalName).OrderBy(x => x)
        );
        Assert.Equal(
            "1B7F3B",
            roundTripped.Waypoints.Single().Extensions.Single().Element(Style + "color")!.Value
        );
        Assert.Equal(
            "C00000",
            roundTripped.Tracks.Single().Extensions.Single().Element(Style + "color")!.Value
        );
        Assert.Equal(
            Style + "seg",
            roundTripped.Tracks.Single().Segments.Single().Extensions.Single().Name
        );
        Assert.Equal(
            "0000FF",
            roundTripped.Routes.Single().Extensions.Single().Element(Style + "color")!.Value
        );
    }

    [Fact]
    public void Gpx11_writes_extensions_where_the_schema_sequences_them()
    {
        var gpx = XDocument.Parse(Parse(Gpx11WithExtensions).ToGpx());
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        // <extensions> comes last in <gpx>, <wpt> and <trkseg>...
        Assert.Equal("extensions", gpx.Root!.Elements().Last().Name.LocalName);
        Assert.Equal("extensions", gpx.Root.Element(ns + "wpt")!.Elements().Last().Name.LocalName);

        // ...but before <rtept> and <trkseg>, which the schema sequences after it.
        var rte = gpx.Root.Element(ns + "rte")!.Elements().Select(x => x.Name.LocalName);
        Assert.Equal(new[] { "name", "extensions", "rtept" }, rte);

        var trk = gpx.Root.Element(ns + "trk")!.Elements().Select(x => x.Name.LocalName);
        Assert.Equal(new[] { "name", "extensions", "trkseg" }, trk);
    }

    [Fact]
    public void Gpx10_carries_extensions_inline()
    {
        // 1.0 has no <extensions> element; its schema ends each type with an
        // xsd:any instead, so the same content sits directly under the parent.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\""
            + " xmlns:gpx_style=\"http://www.topografix.com/GPX/gpx_style/0/2\">"
            + "<wpt lat=\"1\" lon=\"2\"><gpx_style:line><gpx_style:color>1B7F3B</gpx_style:color></gpx_style:line></wpt>"
            + "<trk><name>t</name><gpx_style:line /><trkseg><trkpt lat=\"5\" lon=\"6\" /></trkseg></trk>"
            + "<gpx_style:file>f</gpx_style:file>"
            + "</gpx>";

        var data = Parse(gpx);

        Assert.Equal(Style + "file", data.Extensions.Single().Name);
        Assert.Equal(Style + "line", data.Waypoints.Single().Extensions.Single().Name);
        Assert.Equal(Style + "line", data.Tracks.Single().Extensions.Single().Name);

        var written = XDocument.Parse(data.ToGpx(GpxVersion.Gpx10));
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        // Written back inline, not wrapped.
        Assert.Empty(written.Descendants(ns + "extensions"));
        Assert.Equal(Style + "file", written.Root!.Elements().Last().Name);
        Assert.Equal(
            new[] { "name", "line", "trkseg" },
            written.Root.Element(ns + "trk")!.Elements().Select(x => x.Name.LocalName)
        );
    }

    [Fact]
    public void Gpx10_also_reads_an_extensions_element_it_should_not_have()
    {
        // 1.0 has no <extensions> element, but writers that also emit 1.1 add one
        // anyway. Reading it costs nothing and is the difference between carrying the
        // content through and dropping it silently.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\""
            + " xmlns:gpx_style=\"http://www.topografix.com/GPX/gpx_style/0/2\">"
            + "<wpt lat=\"1\" lon=\"2\">"
            + "<extensions><gpx_style:line><gpx_style:color>1B7F3B</gpx_style:color></gpx_style:line></extensions>"
            + "</wpt>"
            + "</gpx>";

        var data = Parse(gpx);
        var extension = data.Waypoints.Single().Extensions.Single();

        Assert.Equal(Style + "line", extension.Name);
        Assert.Equal("1B7F3B", extension.Element(Style + "color")!.Value);

        // Written back inline, where the 1.0 schema puts it.
        var written = XDocument.Parse(data.ToGpx(GpxVersion.Gpx10));
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        Assert.Empty(written.Descendants(ns + "extensions"));
        Assert.Equal(Style + "line", written.Root!.Element(ns + "wpt")!.Elements().Last().Name);
    }

    [Fact]
    public void Gpx10_does_not_turn_an_unprefixed_extension_into_a_gpx_element()
    {
        // A child of <extensions> written without a prefix inherits the GPX namespace.
        // 1.0 writes extensions inline, so carrying this one through would put a bare
        // <ele> among the waypoint's own children, where nothing distinguishes it from
        // a real elevation - the waypoint would come back 38 metres up and 3D. Neither
        // version admits such an element, so it is dropped instead.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\">"
            + "<wpt lat=\"1\" lon=\"2\"><name>wp</name>"
            + "<extensions><ele>38</ele></extensions>"
            + "</wpt>"
            + "</gpx>";

        var waypoint = Parse(gpx).Waypoints.Single();

        Assert.Empty(waypoint.Extensions);
        Assert.False(waypoint.Coordinate.Is3D);

        var roundTripped = Parse(Parse(gpx).ToGpx(GpxVersion.Gpx10)).Waypoints.Single();
        Assert.False(roundTripped.Coordinate.Is3D);
    }

    [Fact]
    public void A_track_segment_holding_only_extensions_is_kept()
    {
        // An empty <trkseg> is dropped, as it always was. One that carries extensions
        // is not empty, and dropping it would lose them.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\""
            + " xmlns:gpx_style=\"http://www.topografix.com/GPX/gpx_style/0/2\">"
            + "<trk><name>t</name>"
            + "<trkseg><extensions><gpx_style:seg>s</gpx_style:seg></extensions></trkseg>"
            + "<trkseg />"
            + "</trk>"
            + "</gpx>";

        var segments = Parse(gpx).Tracks.Single().Segments;

        Assert.Single(segments);
        Assert.Empty(segments[0].Waypoints);
        Assert.Equal(Style + "seg", segments[0].Extensions.Single().Name);
    }

    [Fact]
    public void Extensions_cross_between_the_two_versions()
    {
        // A 1.1 file written as 1.0 keeps its extensions, and the other way round.
        var asGpx10 = Parse(Parse(Gpx11WithExtensions).ToGpx(GpxVersion.Gpx10));

        Assert.Equal(Style + "line", asGpx10.Waypoints.Single().Extensions.Single().Name);
        Assert.Equal(Style + "line", asGpx10.Tracks.Single().Extensions.Single().Name);

        var backToGpx11 = Parse(asGpx10.ToGpx());
        Assert.Equal(Style + "line", backToGpx11.Tracks.Single().Extensions.Single().Name);
    }

    [Fact]
    public void A_segment_holding_only_extensions_does_not_survive_gpx10_at_all()
    {
        // Not just its extensions: with them gone the segment has no content GPX 1.0
        // can express, so it is written as an empty <trkseg /> and read back as
        // nothing. Recorded because the loss is larger than the extensions themselves.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\""
            + " xmlns:gpx_style=\"http://www.topografix.com/GPX/gpx_style/0/2\">"
            + "<trk><name>t</name>"
            + "<trkseg><extensions><gpx_style:seg>s</gpx_style:seg></extensions></trkseg>"
            + "<trkseg><trkpt lat=\"5\" lon=\"6\" /></trkseg>"
            + "</trk>"
            + "</gpx>";

        var data = Parse(gpx);
        Assert.Equal(2, data.Tracks.Single().Segments.Count);

        // The one carrying points survives; the one carrying only extensions does not.
        var asGpx10 = Parse(data.ToGpx(GpxVersion.Gpx10));
        var segment = Assert.Single(asGpx10.Tracks.Single().Segments);
        Assert.Single(segment.Waypoints);
    }

    [Fact]
    public void A_track_segments_extensions_are_dropped_when_writing_gpx10()
    {
        // The one place 1.0 has no room for: its schema ends <trkseg> with <trkpt>
        // and admits no foreign element after it.
        var asGpx10 = Parse(Parse(Gpx11WithExtensions).ToGpx(GpxVersion.Gpx10));

        Assert.Empty(asGpx10.Tracks.Single().Segments.Single().Extensions);
    }

    [Fact]
    public void The_garmin_extensions_in_the_reference_files_are_read()
    {
        var fileInfo = GetReferenceFileDirectory("gpx")
            .EnumerateFiles("umsonstdraussen.gpx")
            .Single();

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var data = new Gpx11Serializer().DeSerialize(new StreamWrapper(stream));

        var extension = data!
            .Waypoints.Select(x => x.Extensions.FirstOrDefault())
            .First(x => x != null)!;

        Assert.Equal(Garmin + "WaypointExtension", extension.Name);
        Assert.Equal(
            "Freising",
            extension.Element(Garmin + "Address")!.Element(Garmin + "City")!.Value
        );
    }

    [Fact]
    public void Serializing_does_not_reach_back_into_the_data_it_was_given()
    {
        // The elements handed to the writer are copied rather than re-parented, so a
        // caller's GpsData is not quietly rebuilt into the document being written -
        // and serializing twice gives the same answer.
        var data = Parse(Gpx11WithExtensions);
        var extension = data.Tracks.Single().Extensions.Single();

        var first = data.ToGpx();
        Assert.Null(extension.Parent);
        Assert.Same(extension, data.Tracks.Single().Extensions.Single());
        Assert.Equal(first, data.ToGpx());
    }

    [Fact]
    public void A_document_holding_no_extensions_writes_none()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(1, 2));

        Assert.DoesNotContain("<extensions", data.ToGpx());
    }

    [Fact]
    public void An_empty_extensions_element_is_read_as_none()
    {
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<wpt lat=\"1\" lon=\"2\"><extensions></extensions></wpt>"
            + "</gpx>";

        Assert.Empty(Parse(gpx).Waypoints.Single().Extensions);
    }

    [Fact]
    public void Extensions_added_by_hand_are_written()
    {
        var data = new GpsData();
        var waypoint = new Waypoint(1, 2);
        waypoint.Extensions.Add(
            new XElement(Style + "line", new XElement(Style + "color", "FF0000"))
        );
        data.Waypoints.Add(waypoint);

        var roundTripped = Parse(data.ToGpx());

        Assert.Equal(
            "FF0000",
            roundTripped.Waypoints.Single().Extensions.Single().Element(Style + "color")!.Value
        );
    }
}
