using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class SkyDemonFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void CanParse()
    {
        var fileInfo =
            GetReferenceFileDirectory("skydemon").EnumerateFiles().First(x => x.Name == "skydemon.flightplan");

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var file = new SkyDemonFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));
        
        Assert.NotNull(file);
        Assert.Single(file.Routes);
        Assert.Equal(4, file.Routes[0].Waypoints.Count);
    }
}