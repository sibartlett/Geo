using System.IO;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Gps.Serialization
{
    [TestFixture]
    public class GpxSerializerTests : SerializerTestFixtureBase
    {
        [Test]
        public void CanParseAll()
        {
            var gpx10 = new Gpx10Serializer();
            var gpx11 = new Gpx11Serializer();
            var dir = GetReferenceFileDirectory("gpx").EnumerateFiles();
            foreach (var fileInfo in dir)
            {
                using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
                {
                    if (gpx10.CanDeSerialize(stream))
                    {
                        var data = gpx10.DeSerialize(stream);
                        data.ToGpx();
                        data.ToGpx(1);
                    }
                    else if (gpx11.CanDeSerialize(stream))
                    {
                        var data = gpx11.DeSerialize(stream);
                        data.ToGpx();
                        data.ToGpx(1); 
                    }
                    else
                    {
                        Assert.True(false, fileInfo.Name);
                    }
                }
            }
        }
    }
}
