using System.IO;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Geo.Gps.Serialization
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
                    var streamWrapper = new StreamWrapper(stream);
                    if (gpx10.CanDeSerialize(streamWrapper))
                    {
                        var data = gpx10.DeSerialize(streamWrapper);
                        data.ToGpx();
                        data.ToGpx(1);
                    }
                    else if (gpx11.CanDeSerialize(streamWrapper))
                    {
                        var data = gpx11.DeSerialize(streamWrapper);
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
