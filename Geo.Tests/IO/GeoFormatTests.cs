using System;
using Geo.Geometries;
using Geo.IO;
using Xunit;

namespace Geo.Tests.IO;

public class GeoFormatTests
{
    [Theory]
    [InlineData("42.294498, -89.637901")]
    [InlineData("12.345N 123.456E")]
    [InlineData("50°03'46.461\"S 125°48'26.533\"E")]
    [InlineData("(42.294498, -89.637901)")]
    public void Detect_identifies_coordinate_strings(string input)
    {
        Assert.Equal(GeoStringFormat.Coordinate, GeoFormat.Detect(input));
    }

    [Theory]
    [InlineData("POINT (30 10)")]
    [InlineData("LINESTRING (30 10, 10 30, 40 40)")]
    [InlineData("POLYGON ((30 10, 40 40, 20 40, 10 20, 30 10))")]
    [InlineData("point (30 10)")]
    public void Detect_identifies_wkt(string input)
    {
        Assert.Equal(GeoStringFormat.Wkt, GeoFormat.Detect(input));
    }

    [Theory]
    [InlineData("{ \"type\": \"Point\", \"coordinates\": [30, 10] }")]
    [InlineData(
        "{ \"type\": \"Feature\", \"geometry\": { \"type\": \"Point\", \"coordinates\": [30, 10] }, \"properties\": {} }"
    )]
    public void Detect_identifies_geojson(string input)
    {
        Assert.Equal(GeoStringFormat.GeoJson, GeoFormat.Detect(input));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    [InlineData("not a geo string")]
    [InlineData("{ \"type\": \"Nonsense\" }")]
    [InlineData("POINT (nope)")]
    public void Detect_returns_unknown_for_unrecognised_input(string input)
    {
        Assert.Equal(GeoStringFormat.Unknown, GeoFormat.Detect(input));
    }

    [Fact]
    public void TryParse_returns_coordinate()
    {
        var success = GeoFormat.TryParse("42.294498, -89.637901", out var result, out var format);

        Assert.True(success);
        Assert.Equal(GeoStringFormat.Coordinate, format);
        var coordinate = Assert.IsType<Coordinate>(result);
        Assert.Equal(42.294498, coordinate.Latitude);
        Assert.Equal(-89.637901, coordinate.Longitude);
    }

    [Fact]
    public void TryParse_returns_wkt_geometry()
    {
        var success = GeoFormat.TryParse("POINT (30 10)", out var result, out var format);

        Assert.True(success);
        Assert.Equal(GeoStringFormat.Wkt, format);
        var point = Assert.IsType<Point>(result);
        Assert.Equal(10, point.Coordinate.Latitude);
        Assert.Equal(30, point.Coordinate.Longitude);
    }

    [Fact]
    public void TryParse_returns_geojson_object()
    {
        var success = GeoFormat.TryParse(
            "{ \"type\": \"Point\", \"coordinates\": [30, 10] }",
            out var result,
            out var format
        );

        Assert.True(success);
        Assert.Equal(GeoStringFormat.GeoJson, format);
        var point = Assert.IsType<Point>(result);
        Assert.Equal(10, point.Coordinate.Latitude);
        Assert.Equal(30, point.Coordinate.Longitude);
    }

    [Fact]
    public void TryParse_returns_false_for_unrecognised_input()
    {
        var success = GeoFormat.TryParse("not a geo string", out var result, out var format);

        Assert.False(success);
        Assert.Null(result);
        Assert.Equal(GeoStringFormat.Unknown, format);
    }

    [Fact]
    public void TryParse_braced_coordinate_falls_back_from_geojson()
    {
        // A braced coordinate string starts with '{' like GeoJSON, but is not valid JSON.
        var success = GeoFormat.TryParse("{42.294498, -89.637901}", out var result, out var format);

        Assert.True(success);
        Assert.Equal(GeoStringFormat.Coordinate, format);
        Assert.IsType<Coordinate>(result);
    }

    [Fact]
    public void Parse_returns_parsed_value()
    {
        var result = GeoFormat.Parse("POINT (30 10)");
        Assert.IsType<Point>(result);
    }

    [Fact]
    public void Parse_null_throws_argument_null_exception()
    {
        Assert.Throws<ArgumentNullException>(() => GeoFormat.Parse(null));
    }

    [Fact]
    public void Parse_unrecognised_format_throws_format_exception()
    {
        Assert.Throws<FormatException>(() => GeoFormat.Parse("not a geo string"));
    }
}
