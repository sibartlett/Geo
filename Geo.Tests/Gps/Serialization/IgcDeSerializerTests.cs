using System.IO;
using System.Linq;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Gps.Serialization
{
    [TestFixture]
    public class IgcDeSerializerTests : SerializerTestFixtureBase
    {
        [Test]
        public void igc2()
        {
            var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
            using (var stream = new FileStream(file.FullName, FileMode.Open))
            {
                var streamWrapper = new StreamWrapper(stream);
                var parser = new IgcDeSerializer();
                var canParse = parser.CanDeSerialize(streamWrapper);
                var result = parser.DeSerialize(streamWrapper);

                Assert.That(canParse, Is.EqualTo(true));
                Assert.That(result.Waypoints.Count, Is.EqualTo(0));
                Assert.That(result.Tracks.Count, Is.EqualTo(1));
                Assert.That(result.Tracks[0].Segments.Count, Is.EqualTo(1));
                Assert.That(result.Tracks[0].Segments[0].Coordinates.Count, Is.EqualTo(9));
            }
        }
    }
}
