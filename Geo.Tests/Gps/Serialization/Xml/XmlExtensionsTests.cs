using System;
using System.Xml.Linq;
using Geo.Gps.Serialization.Xml;
using Xunit;

namespace Geo.Tests.Gps.Serialization.Xml;

public class XmlExtensionsTests
{
    private static XElement Element(string inner)
    {
        return XElement.Parse("<root>" + inner + "</root>");
    }

    [Fact]
    public void Absent_element_reads_as_null()
    {
        var root = Element("<a>1</a>");

        Assert.Null(root.ElementValue("b"));
        Assert.Null(root.DecimalElement("b"));
        Assert.Null(root.DoubleElement("b"));
        Assert.Null(root.DateTimeElement("b"));
    }

    [Fact]
    public void Absent_attribute_reads_as_null()
    {
        var root = Element(string.Empty);

        Assert.Null(root.AttributeValue("a"));
        Assert.Null(root.DecimalAttribute("a"));
    }

    [Fact]
    public void Null_element_reads_as_null()
    {
        // The accessors are called on the result of Element(...), which is null
        // whenever the parent is absent - a PocketFMS file with no <META>, say.
        XElement? absent = null;

        Assert.Null(absent.ElementValue("a"));
        Assert.Null(absent.DecimalElement("a"));
        Assert.Empty(absent.ElementsOrEmpty("a"));
    }

    [Fact]
    public void Unparseable_content_reads_as_null_rather_than_throwing()
    {
        // XmlSerializer skipped members it could not bind, and files in the reference
        // corpus rely on that: a waypoint with a broken <ele> is still a waypoint.
        var root = Element("<a>not a number</a><b>not a date</b>");

        Assert.Null(root.DecimalElement("a"));
        Assert.Null(root.DoubleElement("a"));
        Assert.Null(root.DateTimeElement("b"));
    }

    [Fact]
    public void Empty_element_reads_as_null()
    {
        var root = Element("<a></a><b>   </b>");

        Assert.Null(root.DecimalElement("a"));
        Assert.Null(root.DecimalElement("b"));
    }

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("-2.25", -2.25)]
    [InlineData(" 3 ", 3)]
    public void Decimals_are_read_invariantly(string text, decimal expected)
    {
        Assert.Equal(expected, Element("<a>" + text + "</a>").DecimalElement("a"));
    }

    [Fact]
    public void A_comma_is_not_a_decimal_point()
    {
        // Whatever the ambient culture, "1,5" is not one and a half - reading it as
        // such would move a coordinate.
        Assert.Null(Element("<a>1,5</a>").DecimalElement("a"));
    }

    [Theory]
    [InlineData("2009-01-01T10:00:00", DateTimeKind.Unspecified)]
    [InlineData("2009-01-01T10:00:00Z", DateTimeKind.Utc)]
    [InlineData("2009-01-01T10:00:00.123Z", DateTimeKind.Utc)]
    public void The_time_formats_in_the_reference_files_keep_their_kind(
        string text,
        DateTimeKind expected
    )
    {
        // All three shapes appear in reference/gpx, and the round-trip tests compare
        // the values they produce, so the kind has to survive being read.
        var value = Element("<a>" + text + "</a>").DateTimeElement("a");

        Assert.NotNull(value);
        Assert.Equal(expected, value!.Value.Kind);
        Assert.Equal(
            new DateTime(2009, 1, 1, 10, 0, 0, expected),
            value.Value,
            TimeSpan.FromSeconds(1)
        );
    }

    [Fact]
    public void A_written_value_keeps_its_surrounding_whitespace()
    {
        // A waypoint in the reference files is named "BORAH " - trimming it here made
        // the name that had just been read fail to survive being written back.
        var element = XmlExtensions.OptionalElement("a", "BORAH ");

        Assert.NotNull(element);
        Assert.Equal("BORAH ", element!.Value);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void A_blank_value_is_not_written_at_all(string? value)
    {
        Assert.Null(XmlExtensions.OptionalElement("a", value));
    }
}
