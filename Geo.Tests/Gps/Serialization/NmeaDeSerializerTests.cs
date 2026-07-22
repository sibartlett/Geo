using System.IO;
using System.Linq;
using System.Text;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class NmeaDeSerializerTests : SerializerTestFixtureBase
{
    // The reference file contains only GPGGA fixes, so craft GPWPL waypoint
    // sentences (including a southern/western hemisphere point) to exercise the
    // waypoint path and the sign handling in ConvertOrd.
    private const string GpwplSentences =
        "$GPWPL,5920.7009,N,01803.2938,E,HOME*00\n$GPWPL,3352.000,S,15112.000,W,DEST*00\n";

    // A leading unparseable line and an unsupported GPRMC sentence should both be
    // ignored, leaving a single GPGGA fix.
    private const string MixedNmea =
        "garbage line that is not a sentence\n$GPRMC,104427,A,5920.7009,N,01803.2938,E,0.0,0.0,160701,,*00\n$GPGGA,104427.591,5920.7009,N,01803.2938,E,1,05,3.3,78.2,M,23.2,M,0.0,0000*4A\n";

    private static StreamWrapper Wrap(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new StreamWrapper(new MemoryStream(bytes));
    }

    [Fact]
    public void Stockholm_Walk()
    {
        var file = GetReferenceFileDirectory("nmea")
            .GetFiles()
            .First(x => x.Name == "Stockholm_Walk.nmea");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);
        var parser = new NmeaDeSerializer();
        var canParse = parser.CanDeSerialize(streamWrapper);
        var result = parser.DeSerialize(streamWrapper);

        Assert.True(canParse);
        Assert.Empty(result.Waypoints);
        Assert.Single(result.Tracks);
        Assert.Single(result.Tracks[0].Segments);
        Assert.Equal(674, result.Tracks[0].Segments[0].Waypoints.Count);
    }

    [Fact]
    public void Parses_first_fix_coordinate_elevation_and_time()
    {
        var file = GetReferenceFileDirectory("nmea")
            .GetFiles()
            .First(x => x.Name == "Stockholm_Walk.nmea");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var result = new NmeaDeSerializer().DeSerialize(new StreamWrapper(stream));

        // First GPGGA: $GPGGA,104427.591,5920.7009,N,01803.2938,E,1,05,3.3,78.2,M,...
        var first = result.Tracks[0].Segments[0].Waypoints[0];
        Assert.Equal(59.345015, first.Coordinate.Latitude, 6);
        Assert.Equal(18.054897, first.Coordinate.Longitude, 6);
        Assert.Equal(78.2, ((CoordinateZ)first.Coordinate).Elevation, 3);
        Assert.NotNull(first.TimeUtc);
        Assert.Equal(10, first.TimeUtc.Value.Hour);
        Assert.Equal(44, first.TimeUtc.Value.Minute);
        Assert.Equal(27, first.TimeUtc.Value.Second);
    }

    [Fact]
    public void Parses_gpwpl_waypoint_sentences()
    {
        var result = new NmeaDeSerializer().DeSerialize(Wrap(GpwplSentences));

        Assert.Empty(result.Tracks);
        Assert.Equal(2, result.Waypoints.Count);
        Assert.Equal(59.345015, result.Waypoints[0].Coordinate.Latitude, 6);
        Assert.Equal(18.054897, result.Waypoints[0].Coordinate.Longitude, 6);
        Assert.Equal(-33.866667, result.Waypoints[1].Coordinate.Latitude, 6);
        Assert.Equal(-151.2, result.Waypoints[1].Coordinate.Longitude, 6);
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_non_nmea()
    {
        Assert.False(new NmeaDeSerializer().CanDeSerialize(Wrap("this is not a GPS file")));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_empty_stream()
    {
        Assert.False(new NmeaDeSerializer().CanDeSerialize(Wrap("")));
    }

    [Fact]
    public void DeSerialize_empty_stream_yields_no_tracks_or_waypoints()
    {
        var result = new NmeaDeSerializer().DeSerialize(Wrap(""));

        Assert.NotNull(result);
        Assert.Empty(result.Tracks);
        Assert.Empty(result.Waypoints);
    }

    [Fact]
    public void DeSerialize_ignores_unrecognised_lines()
    {
        var result = new NmeaDeSerializer().DeSerialize(Wrap(MixedNmea));

        Assert.Single(result.Tracks);
        Assert.Single(result.Tracks[0].Segments[0].Waypoints);
    }
}
