using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

// GPX 1.1 allows any number of <link> elements on the file, a waypoint, a route and
// a track, each with a text and a media type. GPX 1.0 has a single <url>/<urlname>
// pair in the same places. Both map onto GpsLink, so the difference between the
// versions shows up as what survives being written.
public class GpxLinksTests : SerializerTestFixtureBase
{
    private static GpsData Parse(string gpx)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var data = GpsData.Parse(stream);
        Assert.NotNull(data);
        return data!;
    }

    private const string Gpx11WithLinks =
        "<?xml version=\"1.0\"?>"
        + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
        + "<metadata><link href=\"https://example.com/file\"><text>The file</text></link></metadata>"
        + "<wpt lat=\"1\" lon=\"2\">"
        + "<link href=\"https://example.com/one\"><text>one</text><type>text/html</type></link>"
        + "<link href=\"https://example.com/two\"><text>two</text></link>"
        + "<link href=\"https://example.com/three\" />"
        + "</wpt>"
        + "<rte><link href=\"https://example.com/rte\"><text>route</text></link>"
        + "<rtept lat=\"3\" lon=\"4\" /></rte>"
        + "<trk><link href=\"https://example.com/trk\"><text>track</text></link>"
        + "<trkseg><trkpt lat=\"5\" lon=\"6\" /></trkseg></trk>"
        + "</gpx>";

    [Fact]
    public void Gpx11_reads_links_from_every_element_that_can_hold_them()
    {
        var data = Parse(Gpx11WithLinks);

        Assert.Equal("https://example.com/file", data.Links.Single().Href);
        Assert.Equal("The file", data.Links.Single().Text);
        Assert.Equal(3, data.Waypoints.Single().Links.Count);
        Assert.Equal("https://example.com/rte", data.Routes.Single().Links.Single().Href);
        Assert.Equal("https://example.com/trk", data.Tracks.Single().Links.Single().Href);
    }

    [Fact]
    public void A_links_text_and_type_are_read()
    {
        var link = Parse(Gpx11WithLinks).Waypoints.Single().Links[0];

        Assert.Equal("https://example.com/one", link.Href);
        Assert.Equal("one", link.Text);
        Assert.Equal("text/html", link.Type);
    }

    [Fact]
    public void A_link_may_have_nothing_but_an_href()
    {
        var link = Parse(Gpx11WithLinks).Waypoints.Single().Links[2];

        Assert.Equal("https://example.com/three", link.Href);
        Assert.Null(link.Text);
        Assert.Null(link.Type);
    }

    [Fact]
    public void A_link_with_no_href_is_skipped()
    {
        // href is use="required", so a link without one could not be written back.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.1\" xmlns=\"http://www.topografix.com/GPX/1/1\">"
            + "<wpt lat=\"1\" lon=\"2\"><link><text>nowhere</text></link></wpt>"
            + "</gpx>";

        Assert.Empty(Parse(gpx).Waypoints.Single().Links);
    }

    [Fact]
    public void Gpx11_links_survive_a_round_trip_whole()
    {
        var data = Parse(Parse(Gpx11WithLinks).ToGpx());

        Assert.Equal("The file", data.Links.Single().Text);

        var links = data.Waypoints.Single().Links;
        Assert.Equal(3, links.Count);
        Assert.Equal("text/html", links[0].Type);
        Assert.Equal("two", links[1].Text);
        Assert.Null(links[2].Text);

        Assert.Equal("route", data.Routes.Single().Links.Single().Text);
        Assert.Equal("track", data.Tracks.Single().Links.Single().Text);
    }

    [Fact]
    public void Gpx11_writes_links_where_the_schema_sequences_them()
    {
        var gpx = XDocument.Parse(Parse(Gpx11WithLinks).ToGpx());
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        // In <wpt>, <link> follows <desc> and precedes <extensions>.
        Assert.Equal(
            new[] { "link", "link", "link" },
            gpx.Root!.Element(ns + "wpt")!.Elements().Select(x => x.Name.LocalName)
        );

        // In <rte> and <trk> it comes before <rtept>/<trkseg>.
        Assert.Equal(
            new[] { "link", "rtept" },
            gpx.Root.Element(ns + "rte")!.Elements().Select(x => x.Name.LocalName)
        );
        Assert.Equal(
            new[] { "link", "trkseg" },
            gpx.Root.Element(ns + "trk")!.Elements().Select(x => x.Name.LocalName)
        );
    }

    [Fact]
    public void Gpx10_carries_a_link_as_url_and_urlname()
    {
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\">"
            + "<url>https://example.com/file</url><urlname>The file</urlname>"
            + "<wpt lat=\"1\" lon=\"2\">"
            + "<url>https://example.com/wpt</url><urlname>The waypoint</urlname>"
            + "</wpt>"
            + "</gpx>";

        var data = Parse(gpx);

        Assert.Equal("https://example.com/file", data.Links.Single().Href);
        Assert.Equal("The file", data.Links.Single().Text);
        Assert.Equal("The waypoint", data.Waypoints.Single().Links.Single().Text);

        // And back out again as the same pair.
        var written = XDocument.Parse(data.ToGpx(GpxVersion.Gpx10));
        XNamespace ns = "http://www.topografix.com/GPX/1/0";

        Assert.Empty(written.Descendants(ns + "link"));
        Assert.Equal(
            new[] { "url", "urlname" },
            written.Root!.Element(ns + "wpt")!.Elements().Select(x => x.Name.LocalName)
        );
    }

    [Fact]
    public void Writing_gpx10_keeps_only_the_first_link_and_no_type()
    {
        // 1.0 has one <url> per element and no media type, so this is where the two
        // versions genuinely differ in what they can hold.
        var asGpx10 = Parse(Parse(Gpx11WithLinks).ToGpx(GpxVersion.Gpx10));
        var link = asGpx10.Waypoints.Single().Links.Single();

        Assert.Equal("https://example.com/one", link.Href);
        Assert.Equal("one", link.Text);
        Assert.Null(link.Type);
    }

    [Fact]
    public void Links_cross_from_gpx10_up_to_gpx11()
    {
        var gpx =
            "<?xml version=\"1.0\"?>"
            + "<gpx version=\"1.0\" xmlns=\"http://www.topografix.com/GPX/1/0\">"
            + "<wpt lat=\"1\" lon=\"2\">"
            + "<url>https://example.com/wpt</url><urlname>The waypoint</urlname>"
            + "</wpt>"
            + "</gpx>";

        var asGpx11 = Parse(Parse(gpx).ToGpx());
        var link = asGpx11.Waypoints.Single().Links.Single();

        Assert.Equal("https://example.com/wpt", link.Href);
        Assert.Equal("The waypoint", link.Text);
    }

    [Fact]
    public void A_document_with_no_links_writes_none()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(1, 2));

        Assert.DoesNotContain("<link", data.ToGpx());
        Assert.DoesNotContain("<url", data.ToGpx(GpxVersion.Gpx10));
    }

    [Fact]
    public void The_multiple_links_reference_file_keeps_all_four()
    {
        // The file exists for this: four links on one waypoint, one of them with no
        // text. Only the first used to be read, and only its href.
        var fileInfo = GetReferenceFileDirectory("gpx")
            .EnumerateFiles("multiple-links.gpx")
            .Single();

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var data = GpsData.Parse(stream);

        var links = data!.Waypoints.Single().Links;

        Assert.Equal(4, links.Count);
        Assert.Equal(new[] { "one", "two dots", null, "three dots" }, links.Select(x => x.Text));
        Assert.Equal(4, Parse(data.ToGpx()).Waypoints.Single().Links.Count);
    }
}
