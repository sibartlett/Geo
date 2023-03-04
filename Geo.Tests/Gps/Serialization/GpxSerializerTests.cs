using System.IO;
using System.Text;
using Geo.Abstractions.Interfaces;
using Geo.Gps;
using Geo.Gps.Serialization;
using NUnit.Framework;

namespace Geo.Tests.Gps.Serialization;

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
            using (var stream = new FileStream(fileInfo.FullName, FileMode.Open))
            {
                var streamWrapper = new StreamWrapper(stream);
                if (gpx10.CanDeSerialize(streamWrapper))
                {
                    var data = gpx10.DeSerialize(streamWrapper);
                    var gpxData = data.ToGpx();
                    Compare(gpx11, data, gpxData);

                    gpxData = data.ToGpx(1);
                    Compare(gpx10, data, gpxData);
                }
                else if (gpx11.CanDeSerialize(streamWrapper))
                {
                    var data = gpx11.DeSerialize(streamWrapper);
                    var gpxData = data.ToGpx();
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

    private void Compare(Gpx10Serializer serializer, GpsData data, string gpxData)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData)))
        {
            var data2 = serializer.DeSerialize(new StreamWrapper(stream));
            Compare(data, data2);
        }
    }

    private void Compare(Gpx11Serializer serializer, GpsData data, string gpxData)
    {
        using (var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpxData)))
        {
            var data2 = serializer.DeSerialize(new StreamWrapper(stream));
            Compare(data, data2);
        }
    }

    private void Compare(GpsData data, GpsData data2)
    {
        Assert.AreEqual(data.Metadata.Count, data2.Metadata.Count);
        foreach (var entry in data.Metadata) Assert.AreEqual(entry.Value, data2.Metadata[entry.Key]);

        Assert.AreEqual(data.Tracks.Count, data2.Tracks.Count);
        for (var i = 0; i < data.Tracks.Count; i++)
        {
            var track1 = data.Tracks[i];
            var track2 = data2.Tracks[i];

            Assert.AreEqual(track1.Metadata.Count, track2.Metadata.Count);
            foreach (var entry in track1.Metadata) Assert.AreEqual(entry.Value, track2.Metadata[entry.Key]);

            Assert.AreEqual(track1.Segments.Count, track2.Segments.Count);
            for (var s = 0; s < track1.Segments.Count; s++)
            {
                var segment1 = track1.Segments[s];
                var segment2 = track2.Segments[s];

                Assert.AreEqual(segment1.Waypoints.Count, segment2.Waypoints.Count);
                for (var f = 0; f < segment1.Waypoints.Count; f++)
                {
                    var f1 = segment1.Waypoints[f];
                    var f2 = segment2.Waypoints[f];

                    Compare(f1.Point.Coordinate, f2.Point.Coordinate);
                    Assert.AreEqual(f1.TimeUtc, f2.TimeUtc);
                }
            }
        }

        Assert.AreEqual(data.Waypoints.Count, data2.Waypoints.Count);
        for (var i = 0; i < data.Waypoints.Count; i++)
        {
            var wp1 = data.Waypoints[i];
            var wp2 = data2.Waypoints[i];

            Assert.AreEqual(wp1.Name, wp2.Name);
            Assert.AreEqual(wp1.Description, wp2.Description);
            Assert.AreEqual(wp1.Comment, wp2.Comment);
            Compare(wp1.Coordinate, wp2.Coordinate);
        }

        Assert.AreEqual(data.Routes.Count, data2.Routes.Count);
        for (var i = 0; i < data.Routes.Count; i++)
        {
            var r1 = data.Routes[i];
            var r2 = data2.Routes[i];

            Assert.AreEqual(r1.Metadata.Count, r2.Metadata.Count);
            foreach (var entry in r1.Metadata) Assert.AreEqual(entry.Value, r2.Metadata[entry.Key]);

            Assert.AreEqual(r1.Waypoints.Count, r2.Waypoints.Count);
            for (var c = 0; c < r1.Waypoints.Count; c++) Compare(r1.Waypoints[c], r2.Waypoints[c]);
        }
    }

    private static void Compare(Waypoint wp1, Waypoint wp2)
    {
        Compare(wp1.Coordinate, wp2.Coordinate);

        Assert.AreEqual(wp1.Name, wp2.Name);
        Assert.AreEqual(wp1.Description, wp2.Description);
        Assert.AreEqual(wp1.Comment, wp2.Comment);
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