using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class IgcDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void igc2()
    {
        var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);
        var parser = new IgcDeSerializer();
        var canParse = parser.CanDeSerialize(streamWrapper);
        var result = parser.DeSerialize(streamWrapper);

        Assert.True(canParse);
        Assert.Empty(result.Waypoints);
        Assert.Single(result.Tracks);
        Assert.Single(result.Tracks[0].Segments);
        Assert.Equal(9, result.Tracks[0].Segments[0].Waypoints.Count);
    }
}
