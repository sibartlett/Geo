using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Gps.Serialization
{
    [TestFixture]
    public class SkyDemonFlightplanDeSerializerTests : SerializerTestFixtureBase
    {
        [Test]
        public void CanParse()
        {
            var fileInfo =
                GetReferenceFileDirectory("skydemon").EnumerateFiles().First(x => x.Name == "skydemon.flightplan");

            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                var file = new SkyDemonFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));
                Assert.That(file, Is.Not.Null);
                Assert.That(file.Routes.Count, Is.EqualTo(1));
                Assert.That(file.Routes[0].LineString.Coordinates.Count, Is.EqualTo(4));
            }
        }
    }
}
