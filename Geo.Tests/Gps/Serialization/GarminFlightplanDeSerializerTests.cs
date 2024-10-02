using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GarminFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void CanParse()
    {
        var fileInfo =
            GetReferenceFileDirectory("garmin").EnumerateFiles().First(x => x.Name == "garmin.fpl");

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var file = new GarminFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));
        
        Assert.NotNull(file);
        Assert.Single(file.Routes);
        Assert.Equal(5, file.Routes[0].Waypoints.Count);
    }
}