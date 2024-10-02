using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class PocketFmsFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void CanParse()
    {
        var fileInfo =
            GetReferenceFileDirectory("pocketfms").EnumerateFiles().First(x => x.Name == "pocketfms_fp.xml");

        using var stream = new FileStream(fileInfo.FullName, FileMode.Open);
        var file = new PocketFmsFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));
        Assert.NotNull(file);
    }
}