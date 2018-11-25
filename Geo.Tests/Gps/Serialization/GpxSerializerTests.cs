using System.IO;
using System.Text;
using Geo.Abstractions.Interfaces;
using Geo.Gps;
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
                    var streamWrapper = new StreamWrapper(stream);
                    if (gpx10.CanDeSerialize(streamWrapper))
                    {
                        var data = gpx10.DeSerialize(streamWrapper);
                        string gpxData = data.ToGpx();
                        Compare(gpx11, data, gpxData);

                        gpxData = data.ToGpx(1);
                        Compare(gpx10, data, gpxData);
                    }
                    else if (gpx11.CanDeSerialize(streamWrapper))
                    {
                        var data = gpx11.DeSerialize(streamWrapper);
                        string gpxData = data.ToGpx();
                        Compare(gpx11, data, gpxData);

                        gpxData = data.ToGpx(1);
                        Compare(gpx10, data, gpxData);
                    }
                    else
                    {
                        Assert.True(false, fileInfo.Name);
                    }
                }
            }
        }

        void Compare(Gpx10Serializer serializer, GpsData data, string gpxData)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData)))
            {
                GpsData data2 = serializer.DeSerialize(new StreamWrapper(stream));
                Compare(data, data2);
            }
        }

        void Compare(Gpx11Serializer serializer, GpsData data, string gpxData)
        {
            using (MemoryStream stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData)))
            {
                GpsData data2 = serializer.DeSerialize(new StreamWrapper(stream));
                Compare(data, data2);
            }
        }

        void Compare(GpsData data, GpsData data2)
        {
            Assert.AreEqual(data.Metadata.Count, data2.Metadata.Count);
            foreach (var entry in data.Metadata)
            {
                Assert.AreEqual(entry.Value, data2.Metadata[entry.Key]);
            }

            Assert.AreEqual(data.Tracks.Count, data2.Tracks.Count);
            for (int i = 0; i < data.Tracks.Count; i++)
            {
                Track track1 = data.Tracks[i];
                Track track2 = data2.Tracks[i];

                Assert.AreEqual(track1.Metadata.Count, track2.Metadata.Count);
                foreach (var entry in track1.Metadata)
                {
                    Assert.AreEqual(entry.Value, track2.Metadata[entry.Key]);
                }

                Assert.AreEqual(track1.Segments.Count, track2.Segments.Count);
                for (int s = 0; s < track1.Segments.Count; s++)
                {
                    TrackSegment segment1 = track1.Segments[s];
                    TrackSegment segment2 = track2.Segments[s];

                    Assert.AreEqual(segment1.Fixes.Count, segment2.Fixes.Count);
                    for (int f = 0; f < segment1.Fixes.Count; f ++)
                    {
                        Fix f1 = segment1.Fixes[f];
                        Fix f2 = segment2.Fixes[f];

                        Compare(f1.Coordinate, f2.Coordinate);
                        Assert.AreEqual(f1.TimeUtc, f2.TimeUtc);
                    }
                }
            }

            Assert.AreEqual(data.Waypoints.Count, data2.Waypoints.Count);
            for (int i = 0; i < data.Waypoints.Count; i++)
            {
                Waypoint wp1 = data.Waypoints[i];
                Waypoint wp2 = data2.Waypoints[i];

                Assert.AreEqual(wp1.Name, wp2.Name);
                Assert.AreEqual(wp1.Description, wp2.Description);
                Assert.AreEqual(wp1.Comment, wp2.Comment);
                Compare(wp1.Coordinate, wp2.Coordinate);
            }

            Assert.AreEqual(data.Routes.Count, data2.Routes.Count);
            for (int i = 0; i < data.Routes.Count; i++)
            {
                Route r1 = data.Routes[i];
                Route r2 = data2.Routes[i];

                Assert.AreEqual(r1.Metadata.Count, r2.Metadata.Count);
                foreach (var entry in r1.Metadata)
                {
                    Assert.AreEqual(entry.Value, r2.Metadata[entry.Key]);
                }

                Assert.AreEqual(r1.Coordinates.Count, r2.Coordinates.Count);
                for (int c = 0; c < r1.Coordinates.Count; c ++)
                {
                    Compare(r1.Coordinates[c], r2.Coordinates[c]);
                }
            }
        }

        private static void Compare(Coordinate coord1, Coordinate coord2)
        {
            Assert.AreEqual(coord1.Latitude, coord2.Latitude);
            Assert.AreEqual(coord1.Longitude, coord2.Longitude);

            Assert.AreEqual(coord1.Is3D, coord2.Is3D);
            if (coord1.Is3D)
                Assert.AreEqual(((Is3D)coord1).Elevation, ((Is3D)coord2).Elevation);

            Assert.AreEqual(coord1.IsMeasured, coord2.IsMeasured);
            if (coord1.IsMeasured)
                Assert.AreEqual(((IsMeasured)coord1).Measure, ((IsMeasured)coord2).Measure);
        }
    }
}
