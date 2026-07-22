using System.IO;
using System.Linq;
using System.Text;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class IgcDeSerializerTests : SerializerTestFixtureBase
{
    private static StreamWrapper Wrap(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new StreamWrapper(new MemoryStream(bytes));
    }

    [Fact]
    public void igc2()
    {
        var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var streamWrapper = new StreamWrapper(stream);
        var parser = new IgcDeSerializer();
        var canParse = parser.CanDeSerialize(streamWrapper);
        var result = parser.DeSerialize(streamWrapper);

        Assert.True(canParse);
        Assert.Empty(result.Waypoints);
        Assert.Single(result.Tracks);
        Assert.Single(result.Tracks[0].Segments);
        Assert.Equal(9, result.Tracks[0].Segments[0].Waypoints.Count);
    }

    [Fact]
    public void Parses_header_metadata()
    {
        var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var result = new IgcDeSerializer().DeSerialize(new StreamWrapper(stream));

        Assert.Equal("Schleicher ASH-25", result.Metadata.Attribute(x => x.Vehicle.Model));
        Assert.Equal("ABCD-1234", result.Metadata.Attribute(x => x.Vehicle.Identifier));
        Assert.Equal("Bill Bloggs", result.Metadata.Attribute(x => x.Vehicle.Crew1));
    }

    [Fact]
    public void Parses_first_fix_coordinate_elevation_and_time()
    {
        var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
        using var stream = new FileStream(file.FullName, FileMode.Open);
        var result = new IgcDeSerializer().DeSerialize(new StreamWrapper(stream));

        // HFDTE160701 -> 2001-07-16; first B record:
        // B1602405407121N00249342WA0028000421 -> 16:02:40, 54 07.121'N 002 49.342'W, gpsAlt 421
        var first = result.Tracks[0].Segments[0].Waypoints[0];
        Assert.Equal(54.118683, first.Coordinate.Latitude, 6);
        Assert.Equal(-2.822367, first.Coordinate.Longitude, 6);
        Assert.Equal(421d, ((CoordinateZ)first.Coordinate).Elevation, 3);
        Assert.NotNull(first.TimeUtc);
        Assert.Equal(2001, first.TimeUtc.Value.Year);
        Assert.Equal(7, first.TimeUtc.Value.Month);
        Assert.Equal(16, first.TimeUtc.Value.Day);
        Assert.Equal(16, first.TimeUtc.Value.Hour);
        Assert.Equal(2, first.TimeUtc.Value.Minute);
        Assert.Equal(40, first.TimeUtc.Value.Second);
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_non_igc()
    {
        Assert.False(new IgcDeSerializer().CanDeSerialize(Wrap("no b-records here")));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_empty_stream()
    {
        Assert.False(new IgcDeSerializer().CanDeSerialize(Wrap("")));
    }

    [Fact]
    public void DeSerialize_empty_stream_yields_no_tracks()
    {
        var result = new IgcDeSerializer().DeSerialize(Wrap(""));

        Assert.NotNull(result);
        Assert.Empty(result.Tracks);
        Assert.Empty(result.Waypoints);
    }
}
