using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Geo.Geometries;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

// Validates what the GPX serializers write against the official schemas in
// reference/schemas.
//
// The round-trip tests only ever compare Geo's output against Geo's own reader, so
// the two could agree on something GPX does not allow and nothing would notice.
// That is how <gpx> came to be written without its required creator attribute, and
// how the serialization this replaced came to emit a waypoint's inherited children
// before its own - both produced documents no other GPX reader was obliged to
// accept.
public class GpxSchemaValidationTests : SerializerTestFixtureBase
{
    private const string Gpx10Namespace = "http://www.topografix.com/GPX/1/0";
    private const string Gpx11Namespace = "http://www.topografix.com/GPX/1/1";

    private XmlSchemaSet LoadSchemas()
    {
        var schemas = new XmlSchemaSet();
        var directory = GetReferenceFileDirectory("schemas");

        foreach (var name in new[] { "gpx10.xsd", "gpx11.xsd" })
        {
            using var reader = XmlReader.Create(Path.Combine(directory.FullName, name));
            schemas.Add(null, reader);
        }

        schemas.Compile();
        return schemas;
    }

    /// <summary>
    /// Every complaint the schemas make about <paramref name="gpx" />, except the one
    /// they are in no position to judge.
    /// </summary>
    /// <remarks>
    /// GPX 1.0 declares its extension points as <c>xsd:any</c> without a
    /// <c>processContents</c>, which defaults to <c>strict</c> - so validating a 1.0
    /// document that carries any extension at all fails unless every vendor's schema is
    /// loaded too. Errors reported against an element outside the GPX namespace are
    /// therefore ignored; they say only that Garmin's schema was not supplied, which is
    /// a property of GPX 1.0 rather than anything wrong with the document.
    /// <para>
    /// This stays narrow on purpose: it turns on the namespace of the element the
    /// complaint is about, so everything said about a GPX element still counts. In
    /// GPX 1.1 that includes where &lt;extensions&gt; sits, since the element is itself
    /// declared in the GPX namespace. In 1.0 it does not - strict processing rejects a
    /// foreign element for having no schema before it considers where the element
    /// sits - so the placement of a 1.0 extension is pinned by GpxExtensionsTests
    /// rather than here.
    /// </para>
    /// </remarks>
    private List<string> Validate(XmlSchemaSet schemas, string gpx)
    {
        var document = XDocument.Parse(gpx);
        var gpxNamespace = document.Root!.Name.Namespace;
        var errors = new List<string>();

        document.Validate(
            schemas,
            (sender, e) =>
            {
                if (
                    sender is XElement element
                    && element.Name.Namespace != gpxNamespace
                    && e.Exception is XmlSchemaValidationException
                )
                    return;

                errors.Add(e.Message);
            }
        );

        return errors;
    }

    [Fact]
    public void Everything_written_from_the_reference_files_is_valid()
    {
        var schemas = LoadSchemas();
        var gpx10 = new Gpx10Serializer();
        var gpx11 = new Gpx11Serializer();
        var failures = new List<string>();

        foreach (var fileInfo in GetReferenceFileDirectory("gpx").EnumerateFiles())
        {
            using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
            var wrapper = new StreamWrapper(stream);

            var data =
                gpx10.CanDeSerialize(wrapper) ? gpx10.DeSerialize(wrapper)
                : gpx11.CanDeSerialize(wrapper) ? gpx11.DeSerialize(wrapper)
                : null;

            Assert.NotNull(data);

            foreach (
                var (version, written) in new[] { ("1.1", data!.ToGpx()), ("1.0", data.ToGpx(1)) }
            )
                failures.AddRange(
                    Validate(schemas, written)
                        .Select(x => $"{fileInfo.Name} written as GPX {version}: {x}")
                );
        }

        Assert.Empty(failures);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void A_document_with_no_metadata_at_all_is_valid(int version)
    {
        // creator is use="required" in both schemas. It used to be left out whenever
        // the metadata said nothing about what produced the file, so everything Geo
        // wrote from scratch was invalid.
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(53.4808, -2.2426));

        var written = version == 0 ? data.ToGpx() : data.ToGpx(1);

        Assert.Contains("creator=\"Geo\"", written);
        Assert.Empty(Validate(LoadSchemas(), written));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    public void A_fully_populated_document_is_valid(int version)
    {
        var data = new GpsData();
        data.Metadata.Attribute(x => x.Software, "Geo.Tests");
        data.Metadata.Attribute(x => x.Name, "Sample");
        data.Metadata.Attribute(x => x.Description, "A description");
        data.Metadata.Attribute(x => x.Keywords, "one, two");
        data.Metadata.Attribute(x => x.Link, "https://example.com/track");
        data.Metadata.Attribute(x => x.Author.Name, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Author.Email, "ada@example.com");
        data.Metadata.Attribute(x => x.Author.Link, "https://example.com/ada");
        data.Metadata.Attribute(x => x.Copyright.Author, "Ada Lovelace");
        data.Metadata.Attribute(x => x.Copyright.License, "https://example.com/licence");
        data.Metadata.Attribute(x => x.Copyright.Year, "2026");

        data.Waypoints.Add(new Waypoint(new Point(53.4808, -2.2426, 38), "n", "c", "d"));

        var route = new Route();
        route.Metadata.Attribute(x => x.Name, "A route");
        route.Waypoints.Add(new Waypoint(51.5072, -0.1276));
        data.Routes.Add(route);

        var segment = new TrackSegment();
        segment.Waypoints.Add(new Waypoint(53.4808, -2.2426));
        var track = new Track();
        track.Metadata.Attribute(x => x.Name, "A track");
        track.Segments.Add(segment);
        data.Tracks.Add(track);

        var written = version == 0 ? data.ToGpx() : data.ToGpx(1);

        Assert.Empty(Validate(LoadSchemas(), written));
    }

    [Fact]
    public void Extensions_are_written_where_the_schema_allows_them()
    {
        // GPX 1.1 declares its extension points lax, so this is validated for real
        // rather than skipped the way the 1.0 ones have to be: put <extensions> after
        // <rtept> or <trkseg> instead of before, and the schema says so.
        XNamespace style = "http://www.topografix.com/GPX/gpx_style/0/2";
        XElement Line() => new(style + "line", new XElement(style + "color", "C00000"));

        var data = new GpsData();
        data.Extensions.Add(Line());

        var waypoint = new Waypoint(53.4808, -2.2426);
        waypoint.Extensions.Add(Line());
        data.Waypoints.Add(waypoint);

        var route = new Route();
        route.Extensions.Add(Line());
        route.Waypoints.Add(new Waypoint(51.5072, -0.1276));
        data.Routes.Add(route);

        var segment = new TrackSegment();
        segment.Extensions.Add(Line());
        segment.Waypoints.Add(new Waypoint(53.4808, -2.2426));
        var track = new Track();
        track.Extensions.Add(Line());
        track.Segments.Add(segment);
        data.Tracks.Add(track);

        var written = data.ToGpx();

        Assert.Contains("<extensions>", written);
        Assert.Empty(Validate(LoadSchemas(), written));
    }

    [Fact]
    public void The_validation_notices_a_document_the_schema_rejects()
    {
        // Guards the helper itself: an ignore rule that swallowed everything would
        // leave every test above passing on any output at all.
        var gpx =
            "<?xml version=\"1.0\"?>"
            + $"<gpx version=\"1.1\" xmlns=\"{Gpx11Namespace}\">"
            + "<wpt lat=\"1\" lon=\"2\"><name>n</name><ele>3</ele></wpt>"
            + "</gpx>";

        var errors = Validate(LoadSchemas(), gpx);

        // Both the missing creator and <ele> written after <name> rather than before it.
        Assert.Equal(2, errors.Count);
    }

    [Fact]
    public void A_misplaced_extensions_element_is_caught_in_gpx11()
    {
        // The ignore rule is for extension content the schemas cannot resolve, not for
        // the GPX elements around it. <extensions> is itself declared in the GPX
        // namespace, so the schema still has an opinion about where it sits: 1.1
        // sequences it before <rtept>, and this puts it after.
        XNamespace style = "http://www.topografix.com/GPX/gpx_style/0/2";
        var gpx =
            "<?xml version=\"1.0\"?>"
            + $"<gpx version=\"1.1\" creator=\"Geo.Tests\" xmlns=\"{Gpx11Namespace}\""
            + $" xmlns:gpx_style=\"{style}\">"
            + "<rte><rtept lat=\"1\" lon=\"2\" />"
            + "<extensions><gpx_style:line /></extensions></rte>"
            + "</gpx>";

        Assert.NotEmpty(Validate(LoadSchemas(), gpx));
    }

    [Fact]
    public void A_misplaced_extension_is_not_caught_in_gpx10()
    {
        // Recorded because it is a real limit of this test, not an oversight. GPX 1.0
        // has no <extensions> element - foreign elements sit inline - and its xsd:any
        // is strict, so the validator rejects the element for having no schema before
        // it ever considers where the element sits. Nothing the schemas can say about
        // a 1.0 extension's placement survives the ignore rule, so the ordering the
        // 1.0 writer uses is pinned by GpxExtensionsTests instead.
        XNamespace style = "http://www.topografix.com/GPX/gpx_style/0/2";
        var gpx =
            "<?xml version=\"1.0\"?>"
            + $"<gpx version=\"1.0\" creator=\"Geo.Tests\" xmlns=\"{Gpx10Namespace}\""
            + $" xmlns:gpx_style=\"{style}\">"
            + "<rte><rtept lat=\"1\" lon=\"2\" /><gpx_style:line /></rte>"
            + "</gpx>";

        Assert.Empty(Validate(LoadSchemas(), gpx));
    }
}
