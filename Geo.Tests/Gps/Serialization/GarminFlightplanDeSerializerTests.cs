using System.IO;
using System.Linq;
using System.Text;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GarminFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    private static StreamWrapper Wrap(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new StreamWrapper(new MemoryStream(bytes));
    }

    private FileStream OpenReference()
    {
        var fileInfo = GetReferenceFileDirectory("garmin")
            .EnumerateFiles()
            .First(x => x.Name == "garmin.fpl");
        return new FileStream(fileInfo.FullName, FileMode.Open);
    }

    [Fact]
    public void CanParse()
    {
        using var stream = OpenReference();
        var file = new GarminFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));

        Assert.NotNull(file);
        Assert.Single(file.Routes);
        Assert.Equal(5, file.Routes[0].Waypoints.Count);
    }

    [Fact]
    public void Parses_route_name_and_resolved_waypoints()
    {
        using var stream = OpenReference();
        var file = new GarminFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));

        var route = file.Routes[0];
        Assert.Equal("Panshanger - Norwich", route.Metadata.Attribute(x => x.Name));

        // First route point EGLG resolves against the waypoint table.
        Assert.Equal(51.801944, route.Waypoints[0].Coordinate.Latitude, 6);
        Assert.Equal(-0.158333, route.Waypoints[0].Coordinate.Longitude, 6);
    }

    [Fact]
    public void CanDeSerialize_returns_true_for_reference_file()
    {
        using var stream = OpenReference();
        Assert.True(new GarminFlightplanDeSerializer().CanDeSerialize(new StreamWrapper(stream)));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_other_xml()
    {
        Assert.False(new GarminFlightplanDeSerializer().CanDeSerialize(Wrap("<foo />")));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_malformed_xml()
    {
        Assert.False(new GarminFlightplanDeSerializer().CanDeSerialize(Wrap("<broken")));
    }

    [Fact]
    public void DeSerialize_returns_null_for_malformed_xml()
    {
        Assert.Null(new GarminFlightplanDeSerializer().DeSerialize(Wrap("<broken")));
    }
}
