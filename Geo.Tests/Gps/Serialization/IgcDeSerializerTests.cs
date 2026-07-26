using System;
using System.Globalization;
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

    [Fact]
    public void Fixes_past_midnight_are_dated_to_the_following_day()
    {
        // A B-record carries a time of day and nothing else, so a flight that runs past
        // midnight UTC restarts at 00:00:00. Stamped onto the header's date regardless,
        // the track jumped back a day mid-flight and its duration came out negative.
        var igc = string.Join(
            "\n",
            "HFDTE010120",
            "B2358005411868N00248134WA0050000600",
            "B2359595411880N00248137WA0050000600",
            "B0000015411890N00248139WA0050000600",
            "B0001005411900N00248140WA0050000600"
        );

        var result = new IgcDeSerializer().DeSerialize(Wrap(igc));
        var segment = result.Tracks[0].Segments[0];

        Assert.Equal(new DateTime(2020, 1, 1, 23, 58, 0), segment.Waypoints[0].TimeUtc);
        Assert.Equal(new DateTime(2020, 1, 1, 23, 59, 59), segment.Waypoints[1].TimeUtc);
        Assert.Equal(new DateTime(2020, 1, 2, 0, 0, 1), segment.Waypoints[2].TimeUtc);
        Assert.Equal(new DateTime(2020, 1, 2, 0, 1, 0), segment.Waypoints[3].TimeUtc);
        Assert.Equal(TimeSpan.FromMinutes(3), segment.GetDuration());
    }

    [Fact]
    public void A_reused_deserializer_does_not_carry_the_day_over_between_files()
    {
        // GpsData holds the deserializers as shared singletons, so the rollover state has
        // to be per-call: the second file must start on its own header date.
        var igc = string.Join(
            "\n",
            "HFDTE010120",
            "B2358005411868N00248134WA0050000600",
            "B0001005411900N00248140WA0050000600"
        );

        var parser = new IgcDeSerializer();
        parser.DeSerialize(Wrap(igc));
        var second = parser.DeSerialize(Wrap(igc));

        Assert.Equal(
            new DateTime(2020, 1, 1, 23, 58, 0),
            second.Tracks[0].Segments[0].Waypoints[0].TimeUtc
        );
    }

    [Theory]
    [InlineData("en-GB")]
    // Calendars whose current year is not the Gregorian one.
    [InlineData("th-TH")]
    [InlineData("ar-SA")]
    public void Two_digit_year_is_resolved_independently_of_the_current_culture(string culture)
    {
        // HFDTE160701 -> 16 July 2001. The pivot for the file's two-digit year is the
        // current Gregorian year's last two digits; read through the current culture's
        // calendar instead it came from a different era, and every fix in the file
        // landed decades out.
        var previous = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo(culture);

            var file = GetReferenceFileDirectory("igc").GetFiles().First(x => x.Name == "igc2.igc");
            using var stream = new FileStream(file.FullName, FileMode.Open);
            var result = new IgcDeSerializer().DeSerialize(new StreamWrapper(stream));

            var first = result.Tracks[0].Segments[0].Waypoints[0];
            Assert.Equal(new DateTime(2001, 7, 16, 16, 2, 40), first.TimeUtc);
        }
        finally
        {
            CultureInfo.CurrentCulture = previous;
        }
    }
}
