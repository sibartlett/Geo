using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class NmeaDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void Stockholm_Walk()
    {
        var file = GetReferenceFileDirectory("nmea")
            .GetFiles()
            .First(x => x.Name == "Stockholm_Walk.nmea");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);
        var parser = new NmeaDeSerializer();
        var canParse = parser.CanDeSerialize(streamWrapper);
        var result = parser.DeSerialize(streamWrapper);

        Assert.True(canParse);
        Assert.Empty(result.Waypoints);
        Assert.Single(result.Tracks);
        Assert.Single(result.Tracks[0].Segments);
        Assert.Equal(674, result.Tracks[0].Segments[0].Waypoints.Count);
    }
}
