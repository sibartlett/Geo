using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Gps.Serialization
{
    [TestFixture]
    public class NmeaDeSerializerTests : SerializerTestFixtureBase
    {
        [Test]
        public void Stockholm_Walk()
        {
            var file = GetReferenceFileDirectory("nmea").GetFiles().First(x => x.Name == "Stockholm_Walk.nmea");
            using (var stream = new FileStream(file.FullName, FileMode.Open))
            {
                var streamWrapper = new StreamWrapper(stream);
                var parser = new NmeaDeSerializer();
                var canParse = parser.CanDeSerialize(streamWrapper);
                var result = parser.DeSerialize(streamWrapper);

                Assert.That(canParse, Is.EqualTo(true));
                Assert.That(result.Waypoints.Count, Is.EqualTo(0));
                Assert.That(result.Tracks.Count, Is.EqualTo(1));
                Assert.That(result.Tracks[0].Segments.Count, Is.EqualTo(1));
                Assert.That(result.Tracks[0].Segments[0].Fixes.Count, Is.EqualTo(674));
            }
        }
    }
}
