using System.IO;
using System.Linq;
using System.Text;
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
}
