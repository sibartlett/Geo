using System.IO;
using System.Xml;
using Geo.Gps.Serialization.Xml;
using Xunit;

namespace Geo.Tests.Gps.Serialization.Xml;

public class NamespaceCoercingXmlReaderTests
{
    private const string Ns = "http://www.topografix.com/GPX/1/1";

    private static NamespaceCoercingXmlReader Wrap(string xml) =>
        new(XmlReader.Create(new StringReader(xml)), Ns);

    [Fact]
    public void Namespaceless_elements_are_coerced_to_the_fixed_namespace()
    {
        using var reader = Wrap("<gpx><wpt/></gpx>");

        Assert.True(reader.Read()); // <gpx>
        Assert.Equal(XmlNodeType.Element, reader.NodeType);
        Assert.Equal("gpx", reader.LocalName);
        Assert.Equal(Ns, reader.NamespaceURI);

        Assert.True(reader.Read()); // <wpt/>
        Assert.Equal("wpt", reader.LocalName);
        Assert.Equal(Ns, reader.NamespaceURI);
        Assert.True(reader.IsEmptyElement);
    }

    [Fact]
    public void End_elements_are_also_coerced()
    {
        using var reader = Wrap("<gpx></gpx>");

        Assert.True(reader.Read()); // <gpx>
        Assert.True(reader.Read()); // </gpx>
        Assert.Equal(XmlNodeType.EndElement, reader.NodeType);
        Assert.Equal(Ns, reader.NamespaceURI);
    }

    [Fact]
    public void Already_namespaced_elements_are_passed_through_unchanged()
    {
        using var reader = Wrap("<gpx xmlns=\"http://example.com/other\"><wpt/></gpx>");

        Assert.True(reader.Read());
        Assert.Equal("http://example.com/other", reader.NamespaceURI);
    }

    [Fact]
    public void Non_element_nodes_report_the_inner_namespace()
    {
        using var reader = Wrap("<gpx>text</gpx>");

        Assert.True(reader.Read()); // <gpx>
        Assert.True(reader.Read()); // text
        Assert.Equal(XmlNodeType.Text, reader.NodeType);
        Assert.Equal("text", reader.Value);
        // A text node is in no namespace and must not be coerced.
        Assert.Equal(string.Empty, reader.NamespaceURI);
    }

    [Fact]
    public void Attributes_and_reader_members_delegate_to_the_inner_reader()
    {
        using var reader = Wrap("<gpx version=\"1.1\" creator=\"Geo\"/>");

        Assert.True(reader.Read());
        Assert.Equal(2, reader.AttributeCount);
        Assert.Equal("1.1", reader.GetAttribute("version"));
        Assert.Equal("Geo", reader.GetAttribute(1));

        Assert.True(reader.MoveToFirstAttribute());
        Assert.Equal("version", reader.LocalName);
        Assert.True(reader.MoveToNextAttribute());
        Assert.Equal("creator", reader.LocalName);
        Assert.True(reader.MoveToElement());
        Assert.Equal("gpx", reader.LocalName);

        Assert.Equal(0, reader.Depth);
        Assert.False(reader.EOF);
        Assert.NotNull(reader.NameTable);
        Assert.Equal(ReadState.Interactive, reader.ReadState);
    }
}
