using System.IO;
using System.Linq;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GpsDeSerializerTests : SerializerTestFixtureBase
{
    [Fact]
    public void ImageFileTest()
    {
        var file = GetReferenceFileDirectory().GetFiles().First(x => x.Name == "image.png");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var data = GpsData.Parse(stream);

        Assert.Null(data);
    }
}
