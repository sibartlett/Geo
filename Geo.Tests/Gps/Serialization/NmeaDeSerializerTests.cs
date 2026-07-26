using System;
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

    [Fact]
    public void Fixes_past_midnight_are_dated_to_the_following_day()
    {
        // GPGGA carries a time of day and no date, so a track that runs past midnight UTC
        // restarts at 00:00:00. Stamped onto the same day regardless, the track jumped
        // back twenty-four hours mid-recording and its duration came out negative.
        var nmea = string.Join(
            "\n",
            "$GPGGA,235800.00,5920.7009,N,01803.2938,E,1,05,3.3,78.2,M,23.2,M,0.0,0000*4A",
            "$GPGGA,000100.00,5920.7010,N,01803.2939,E,1,05,3.3,78.2,M,23.2,M,0.0,0000*4A"
        );

        var segment = new NmeaDeSerializer().DeSerialize(Wrap(nmea)).Tracks[0].Segments[0];

        Assert.Equal(
            segment.Waypoints[0].TimeUtc!.Value.AddMinutes(3),
            segment.Waypoints[1].TimeUtc
        );
        Assert.Equal(TimeSpan.FromMinutes(3), segment.GetDuration());
    }

    [Fact]
    public void A_reused_deserializer_does_not_carry_the_day_over_between_files()
    {
        // GpsData holds the deserializers as shared singletons, so the rollover state has
        // to be per-call: the second file must start on the same day as the first did.
        var nmea = string.Join(
            "\n",
            "$GPGGA,235800.00,5920.7009,N,01803.2938,E,1,05,3.3,78.2,M,23.2,M,0.0,0000*4A",
            "$GPGGA,000100.00,5920.7010,N,01803.2939,E,1,05,3.3,78.2,M,23.2,M,0.0,0000*4A"
        );

        var parser = new NmeaDeSerializer();
        var first = parser.DeSerialize(Wrap(nmea)).Tracks[0].Segments[0];
        var second = parser.DeSerialize(Wrap(nmea)).Tracks[0].Segments[0];

        Assert.Equal(first.Waypoints[0].TimeUtc, second.Waypoints[0].TimeUtc);
    }

    private static string Gga(
        string latitude,
        string latitudeDirection,
        string longitude,
        string longitudeDirection,
        string time = "104427.591"
    ) =>
        $"$GPGGA,{time},{latitude},{latitudeDirection},{longitude},{longitudeDirection},"
        + "1,05,3.3,78.2,M,23.2,M,0.0,0000*4A";

    private static string Wpl(string latitude, string longitude) =>
        $"$GPWPL,{latitude},N,{longitude},E,HOME*00";

    private static readonly string GoodFix = Gga("5920.7009", "N", "01803.2938", "E");

    [Fact]
    public void One_unreadable_sentence_does_not_lose_the_rest_of_the_log()
    {
        // The whole point of the fix. An NMEA log is a stream of sentences, not a single
        // document, and one of any length is likely to hold a truncated or corrupted line.
        // Reading an ordinate used to throw on such a line, which discarded every fix in the
        // file rather than just the bad one.
        var nmea = string.Join("\n", GoodFix, Gga("5", "N", "01803.2938", "E"), GoodFix);

        var data = new NmeaDeSerializer().DeSerialize(Wrap(nmea));

        Assert.Equal(2, data.Tracks[0].Segments[0].Waypoints.Count);
    }

    [Theory]
    // Shorter than the degrees field the format fixes the width of, so the split ran off the
    // end of the string.
    [InlineData("5", "N", "01803.2938", "E")]
    [InlineData("5920.7009", "N", "01", "E")]
    // Exactly the degrees field, leaving nothing for the minutes and an empty double.Parse.
    [InlineData("51", "N", "01803.2938", "E")]
    [InlineData("5920.7009", "N", "018", "E")]
    // A bare fraction, which the sentence pattern admits.
    [InlineData(".5", "N", "01803.2938", "E")]
    // Degrees the pattern is free to quote but a position cannot hold.
    [InlineData("9900.00", "N", "01803.2938", "E")]
    [InlineData("5920.7009", "N", "19900.00", "E")]
    public void A_fix_whose_ordinates_cannot_be_read_is_skipped(
        string latitude,
        string latitudeDirection,
        string longitude,
        string longitudeDirection
    )
    {
        var nmea = Gga(latitude, latitudeDirection, longitude, longitudeDirection);

        var data = new NmeaDeSerializer().DeSerialize(Wrap(nmea));

        Assert.Empty(data.Tracks);
    }

    [Theory]
    [InlineData("5", "01803.2938")]
    [InlineData("51", "01803.2938")]
    [InlineData("5920.7009", "018")]
    [InlineData("9900.00", "01803.2938")]
    public void A_waypoint_whose_ordinates_cannot_be_read_is_skipped(
        string latitude,
        string longitude
    )
    {
        var data = new NmeaDeSerializer().DeSerialize(Wrap(Wpl(latitude, longitude)));

        Assert.Empty(data.Waypoints);
    }

    [Fact]
    public void A_skipped_sentence_does_not_advance_the_rollover_clock()
    {
        // The clock is asked for a timestamp only once the position is known to be good.
        // Were a skipped sentence to advance it, the 23:00 below would become the time the
        // 11:00 after it is measured against, and 11:00 being earlier would be taken for a
        // new day.
        var nmea = string.Join(
            "\n",
            Gga("5920.7009", "N", "01803.2938", "E", time: "100000.00"),
            Gga("5", "N", "01803.2938", "E", time: "230000.00"),
            Gga("5920.7010", "N", "01803.2939", "E", time: "110000.00")
        );

        var waypoints = new NmeaDeSerializer()
            .DeSerialize(Wrap(nmea))
            .Tracks[0]
            .Segments[0]
            .Waypoints;

        Assert.Equal(2, waypoints.Count);
        Assert.Equal(waypoints[0].TimeUtc!.Value.Date, waypoints[1].TimeUtc!.Value.Date);
        Assert.Equal(TimeSpan.FromHours(1), waypoints[1].TimeUtc - waypoints[0].TimeUtc);
    }

    [Fact]
    public void Southern_and_western_ordinates_are_negated()
    {
        var nmea = string.Join(
            "\n",
            Gga("5920.7009", "S", "01803.2938", "W"),
            Gga("5920.7009", "N", "01803.2938", "E")
        );

        var waypoints = new NmeaDeSerializer()
            .DeSerialize(Wrap(nmea))
            .Tracks[0]
            .Segments[0]
            .Waypoints;

        Assert.Equal(-59.345015, waypoints[0].Coordinate.Latitude, 6);
        Assert.Equal(-18.054897, waypoints[0].Coordinate.Longitude, 6);
        Assert.Equal(59.345015, waypoints[1].Coordinate.Latitude, 6);
        Assert.Equal(18.054897, waypoints[1].Coordinate.Longitude, 6);
    }
}
