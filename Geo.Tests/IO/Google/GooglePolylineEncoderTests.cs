using System;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.IO.Google;
using Xunit;

namespace Geo.Tests.IO.Google;

public class GooglePolylineEncoderTests
{
    [Fact]
    public void Encode()
    {
        var lineString = new LineString(
            new Coordinate(38.5, -120.2),
            new Coordinate(40.7, -120.95),
            new Coordinate(43.252, -126.453)
        );

        var result = new GooglePolylineEncoder().Encode(lineString);

        Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", result);
    }

    [Fact]
    public void Encode_rounds_the_scaled_ordinate_to_the_nearest_unit()
    {
        // Regression: the scaled ordinate must be rounded (as the Google polyline
        // algorithm specifies), not truncated toward zero. 1.234567 * 1e5 = 123456.7,
        // which rounds to 123457; truncation would give 123456 and produce "_cpF_cpF".
        var lineString = new LineString(new Coordinate(1.234567, 1.234567));

        var result = new GooglePolylineEncoder().Encode(lineString);

        Assert.Equal("acpFacpF", result);
    }

    [Fact]
    public void Decode()
    {
        var lineString = new LineString(
            new Coordinate(38.5, -120.2),
            new Coordinate(40.7, -120.95),
            new Coordinate(43.252, -126.453)
        );

        var result = new GooglePolylineEncoder().Decode("_p~iF~ps|U_ulLnnqC_mqNvxq`@");

        Assert.Equal(lineString, result);
    }

    [Fact]
    public void Decode_empty_string_gives_an_empty_linestring()
    {
        Assert.True(new GooglePolylineEncoder().Decode("").IsEmpty);
    }

    [Fact]
    public void Decode_round_trips_the_largest_possible_deltas()
    {
        // Pole to pole and antimeridian to antimeridian is the longest number the
        // encoder can emit; decoding must not reject it as an overflow.
        var encoder = new GooglePolylineEncoder();
        var lineString = new LineString(
            new Coordinate(-90, -180),
            new Coordinate(90, 180),
            new Coordinate(-90, -180)
        );

        Assert.Equal(lineString, encoder.Decode(encoder.Encode(lineString)));
    }

    [Theory]
    // Truncated: the string ends part-way through a number. Decoding used to treat
    // the end of the string as a final chunk of zero, fabricating a coordinate on
    // the null meridian instead of reporting the truncation.
    [InlineData("_p~iF")] // latitude only, longitude missing entirely
    [InlineData("_p~iF~ps")] // longitude cut mid-number
    [InlineData("a")] // a lone continuation chunk
    [InlineData("?")] // a lone terminating chunk
    public void Decode_truncated_polyline_throws_serialization(string polyline)
    {
        Assert.Throws<SerializationException>(() => new GooglePolylineEncoder().Decode(polyline));
    }

    [Theory]
    // Characters below the 63 offset are not polyline data. Their low bits used to be
    // masked into the result, so a stray newline or space silently produced extra,
    // wrong coordinates rather than an error.
    [InlineData("_p~iF~ps|U\n")] // trailing newline, as read from a file
    [InlineData(" _p~iF~ps|U")] // leading space
    [InlineData("_p~iF~ps|U _p~iF~ps|U")] // two polylines separated by a space
    [InlineData("_p~iF\u007f~ps|U")] // DEL, above the encodable range
    public void Decode_invalid_character_throws_serialization(string polyline)
    {
        Assert.Throws<SerializationException>(() => new GooglePolylineEncoder().Decode(polyline));
    }

    [Fact]
    public void Decode_number_too_large_throws_serialization()
    {
        // A runaway run of continuation chunks used to wrap the shift (C# masks it to
        // 0-31) and fold back over the low bits, decoding to (0, 0) with no error.
        Assert.Throws<SerializationException>(() =>
            new GooglePolylineEncoder().Decode("~~~~~~~~~~~~?~~~~~~~~~~~~?")
        );
    }

    [Fact]
    public void Decode_out_of_range_coordinate_throws_serialization()
    {
        // A well-formed polyline can still decode to an impossible position; that is a
        // malformed document, reported like every other decoding failure.
        var encoder = new GooglePolylineEncoder();
        var step = encoder.Encode(new LineString(new Coordinate(50, 0)));

        // Two 50-degree steps north run past the pole.
        Assert.Throws<SerializationException>(() => encoder.Decode(step + step));
    }

    [Fact]
    public void Decode_null_throws_argument_null()
    {
        Assert.Throws<ArgumentNullException>(() => new GooglePolylineEncoder().Decode(null));
    }
}
