using System.IO;
using System.Linq;
using System.Text;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class SkyDemonFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    private static StreamWrapper Wrap(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new StreamWrapper(new MemoryStream(bytes));
    }

    private FileStream OpenReference()
    {
        var fileInfo = GetReferenceFileDirectory("skydemon")
            .EnumerateFiles()
            .First(x => x.Name == "skydemon.flightplan");
        return new FileStream(fileInfo.FullName, FileMode.Open);
    }

    [Fact]
    public void CanParse()
    {
        using var stream = OpenReference();
        var file = new SkyDemonFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));

        Assert.NotNull(file);
        Assert.Single(file.Routes);
        Assert.Equal(4, file.Routes[0].Waypoints.Count);
    }

    [Fact]
    public void Parses_primary_route_start_coordinate()
    {
        using var stream = OpenReference();
        var file = new SkyDemonFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));

        // PrimaryRoute Start="N514807.00 W0000930.00" (DMS with hemisphere prefixes).
        var start = file.Routes[0].Waypoints[0];
        Assert.Equal(51.801944, start.Coordinate.Latitude, 6);
        Assert.Equal(-0.158333, start.Coordinate.Longitude, 6);
    }

    [Fact]
    public void CanDeSerialize_returns_true_for_reference_file()
    {
        using var stream = OpenReference();
        Assert.True(new SkyDemonFlightplanDeSerializer().CanDeSerialize(new StreamWrapper(stream)));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_other_xml()
    {
        Assert.False(new SkyDemonFlightplanDeSerializer().CanDeSerialize(Wrap("<foo />")));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_malformed_xml()
    {
        Assert.False(new SkyDemonFlightplanDeSerializer().CanDeSerialize(Wrap("<broken")));
    }

    [Fact]
    public void DeSerialize_returns_null_for_malformed_xml()
    {
        Assert.Null(new SkyDemonFlightplanDeSerializer().DeSerialize(Wrap("<broken")));
    }

    private static string Plan(string body) =>
        "<?xml version=\"1.0\" encoding=\"utf-8\"?><DivelementsFlightPlanner>"
        + body
        + "</DivelementsFlightPlanner>";

    private static string Route(string start, string to = "N515646.05 W0000106.30") =>
        Plan($"<PrimaryRoute Start=\"{start}\"><RhumbLineRoute To=\"{to}\" /></PrimaryRoute>");

    private static GpsData? DeSerialize(string xml) =>
        new SkyDemonFlightplanDeSerializer().DeSerialize(Wrap(xml));

    [Fact]
    public void A_comma_in_place_of_the_decimal_point_is_not_read_as_a_thousands_separator()
    {
        // The seconds pattern was written (?<s>\d\d.\d\d) with a bare '.', which matches any
        // character, so "N514807,00" matched and its seconds arrived at double.Parse as
        // "07,00" - seven hundred, once thousands separators are allowed. The waypoint came
        // back at 51.994, -0.983 rather than 51.802, -0.158: about 21 km north and 92 km
        // west of where the file put it, with nothing reported.
        Assert.Null(DeSerialize(Route("N514807,00 W0000930,00")));
        Assert.Null(DeSerialize(Route("N514807X00 W0000930X00")));
    }

    [Theory]
    // Whole seconds, and seconds to any number of decimal places, are all valid; only two
    // decimal places used to be accepted, and anything else threw.
    [InlineData("N514807.00 W0000930.00")]
    [InlineData("N514807.0 W0000930.0")]
    [InlineData("N514807 W0000930")]
    [InlineData("N514807.000 W0000930.000")]
    // More than one space between the ordinates, which used to split into an empty entry.
    [InlineData("N514807.00  W0000930.00")]
    [InlineData("  N514807.00 W0000930.00  ")]
    public void Seconds_and_spacing_may_vary(string start)
    {
        var data = DeSerialize(Route(start));

        Assert.NotNull(data);
        var coordinate = data!.Routes[0].Waypoints[0].Coordinate;
        Assert.Equal(51.801944, coordinate.Latitude, 6);
        Assert.Equal(-0.158333, coordinate.Longitude, 6);
    }

    [Theory]
    // No space at all between the ordinates, which used to index past the end of the split.
    [InlineData("N514807.00W0000930.00")]
    // The ordinates the wrong way round, and a lone ordinate - neither matches.
    [InlineData("W0000930.00 N514807.00")]
    [InlineData("N514807.00")]
    // Degrees the pattern accepts but a position cannot hold.
    [InlineData("N994807.00 W0000930.00")]
    [InlineData("N514807.00 W9990930.00")]
    // Not a coordinate at all.
    [InlineData("Panshanger")]
    [InlineData("")]
    public void A_coordinate_that_cannot_be_read_yields_null_rather_than_throwing(string start)
    {
        // Reported the way this deserializer already reports a document it cannot parse.
        // Previously each of these left as an exception - FormatException,
        // IndexOutOfRangeException or ArgumentOutOfRangeException - out of GpsData.Parse.
        Assert.Null(DeSerialize(Route(start)));
    }

    [Fact]
    public void An_unreadable_coordinate_anywhere_in_the_route_yields_null()
    {
        Assert.Null(DeSerialize(Route("N514807.00 W0000930.00", to: "not a coordinate")));
    }

    [Fact]
    public void A_route_holding_only_a_starting_point_is_read()
    {
        // RhumbLineRoute is absent rather than empty for such a route, and was dereferenced.
        var data = DeSerialize(Plan("<PrimaryRoute Start=\"N514807.00 W0000930.00\" />"));

        Assert.NotNull(data);
        Assert.Single(data!.Routes);
        Assert.Single(data.Routes[0].Waypoints);
    }

    [Theory]
    // An absent Start, and an absent To, were both dereferenced.
    [InlineData("<PrimaryRoute><RhumbLineRoute To=\"N515646.05 W0000106.30\" /></PrimaryRoute>")]
    [InlineData(
        "<PrimaryRoute Start=\"N514807.00 W0000930.00\"><RhumbLineRoute Level=\"MSL\" /></PrimaryRoute>"
    )]
    public void A_missing_coordinate_attribute_yields_null(string body)
    {
        Assert.Null(DeSerialize(Plan(body)));
    }

    [Fact]
    public void Only_the_southern_and_western_hemispheres_are_negated()
    {
        // S is negative and E is not, and each ordinate is judged by its own letter. Both
        // were previously tested against "N or E", which happened to work but only because
        // a latitude can never carry an E.
        var southEast = DeSerialize(Route("S332600.00 E0150900.00", to: "S332700.00 E0151000.00"));

        Assert.NotNull(southEast);
        var coordinate = southEast!.Routes[0].Waypoints[0].Coordinate;
        Assert.Equal(-33.433333, coordinate.Latitude, 6);
        Assert.Equal(15.15, coordinate.Longitude, 6);

        var northWest = DeSerialize(Route("N332600.00 W0150900.00", to: "N332700.00 W0151000.00"));

        Assert.NotNull(northWest);
        coordinate = northWest!.Routes[0].Waypoints[0].Coordinate;
        Assert.Equal(33.433333, coordinate.Latitude, 6);
        Assert.Equal(-15.15, coordinate.Longitude, 6);
    }

    [Fact]
    public void A_plan_with_no_routes_yields_no_routes()
    {
        var data = DeSerialize(Plan("<Aircraft Registration=\"G-ABCD\" Type=\"C172\" />"));

        Assert.NotNull(data);
        Assert.Empty(data!.Routes);
        Assert.Equal("G-ABCD", data.Metadata.Attribute(x => x.Vehicle.Identifier));
    }
}
