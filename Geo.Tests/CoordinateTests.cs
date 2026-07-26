using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Geo.Tests;

public class CoordinateTests
{
    [Theory]
    [InlineData("     42.294498        -89.637901         ", 42.294498, -89.637901)]
    [InlineData("12 34.56'N 123 45.55'E", 12.576, 123.75916666666667)]
    [InlineData("12.345°N 123.456°E", 12.345, 123.456)]
    [InlineData("12.345N 123.456E", 12.345, 123.456)]
    [InlineData("12°N 34°W", 12, -34)]
    [InlineData("42.294498, -89.637901", 42.294498, -89.637901)]
    [InlineData("(42.294498, -89.637901)", 42.294498, -89.637901)]
    [InlineData("[42.294498, -89.637901]", 42.294498, -89.637901)]
    [InlineData(" ( 42.294498, -89.637901 ) ", 42.294498, -89.637901)]
    [InlineData("42° 17′ 40″ N, 89° 38′ 16″ W", 42.294444444444444d, -89.637777777777785d)]
    [InlineData("-42° 17′ 40″ N, 89° 38′ 16″ W", -42.294444444444444d, -89.637777777777785d)]
    [InlineData("-42°″, -89°", -42d, -89d)]
    public void Parse(string coordinate, double latitude, double longitude)
    {
        var result = Coordinate.Parse(coordinate);
        Assert.NotNull(result);
        Assert.Equal(result.Latitude, latitude);
        Assert.Equal(result.Longitude, longitude);
    }

    [Fact]
    public void Equality_Elevation()
    {
        Assert.True(
            new CoordinateZ(0, 0, 0).Equals(
                new CoordinateZ(0, 0, 0),
                new SpatialEqualityOptions { UseElevation = true }
            )
        );
        Assert.False(
            new CoordinateZ(0, 0, 0).Equals(
                new CoordinateZ(0, 0, 10),
                new SpatialEqualityOptions { UseElevation = true }
            )
        );
        Assert.True(
            new CoordinateZ(0, 0, 0).Equals(
                new CoordinateZ(0, 0, 10),
                new SpatialEqualityOptions { UseElevation = false }
            )
        );
    }

    [Fact]
    public void Equality_M()
    {
        Assert.True(
            new CoordinateZM(0, 0, 0, 0).Equals(
                new CoordinateZM(0, 0, 0, 0),
                new SpatialEqualityOptions { UseM = true }
            )
        );
        Assert.False(
            new CoordinateZM(0, 0, 0, 0).Equals(
                new CoordinateZM(0, 0, 0, 10),
                new SpatialEqualityOptions { UseM = true }
            )
        );
        Assert.True(
            new CoordinateZM(0, 0, 0, 0).Equals(
                new CoordinateZM(0, 0, 0, 10),
                new SpatialEqualityOptions { UseM = false }
            )
        );
    }

    [Fact]
    public void Equality_M_only_measure_toggle()
    {
        Assert.True(
            new CoordinateM(0, 0, 0).Equals(
                new CoordinateM(0, 0, 0),
                new SpatialEqualityOptions { UseM = true }
            )
        );
        Assert.False(
            new CoordinateM(0, 0, 0).Equals(
                new CoordinateM(0, 0, 10),
                new SpatialEqualityOptions { UseM = true }
            )
        );
        Assert.True(
            new CoordinateM(0, 0, 0).Equals(
                new CoordinateM(0, 0, 10),
                new SpatialEqualityOptions { UseM = false }
            )
        );
    }

    [Fact]
    public void Default_value_equality_holds_for_matching_coordinates()
    {
        var a = new Coordinate(1, 2);
        var b = new Coordinate(1, 2);
        var c = new Coordinate(3, 4);

        Assert.True(a.Equals((object)b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());

        Assert.False(a == c);
        Assert.True(a != c);
    }

    [Fact]
    public void Equality_operators_handle_null()
    {
        var coordinate = new Coordinate(1, 2);

        Assert.True((Coordinate)null == (Coordinate)null);
        Assert.False(coordinate == null);
        Assert.False(null == coordinate);
        Assert.True(coordinate != null);
    }

    [Fact]
    public void Coordinates_of_different_dimensions_are_not_equal()
    {
        var options = new SpatialEqualityOptions();
        var flat = new Coordinate(1, 2);
        var z = new CoordinateZ(1, 2, 0);
        var m = new CoordinateM(1, 2, 0);
        var zm = new CoordinateZM(1, 2, 0, 0);

        Assert.False(flat.Equals(z, options));
        Assert.False(z.Equals(flat, options));
        Assert.False(z.Equals(m, options));
        Assert.False(m.Equals(z, options));
        Assert.False(zm.Equals(z, options));
        Assert.False(zm.Equals(flat, options));
    }

    [Fact]
    public void GetHashCode_matches_equality_across_options()
    {
        // Elevation excluded when UseElevation is false.
        var without3D = new SpatialEqualityOptions { UseElevation = false };
        Assert.True(new CoordinateZ(1, 2, 100).Equals(new CoordinateZ(1, 2, 200), without3D));
        Assert.Equal(
            new CoordinateZ(1, 2, 100).GetHashCode(without3D),
            new CoordinateZ(1, 2, 200).GetHashCode(without3D)
        );

        // Measure excluded when UseM is false (measure must not be gated on UseElevation).
        var withoutM = new SpatialEqualityOptions { UseM = false };
        Assert.True(new CoordinateM(1, 2, 100).Equals(new CoordinateM(1, 2, 200), withoutM));
        Assert.Equal(
            new CoordinateM(1, 2, 100).GetHashCode(withoutM),
            new CoordinateM(1, 2, 200).GetHashCode(withoutM)
        );

        // Pole longitudes collapse to a single hash bucket.
        var options = new SpatialEqualityOptions();
        Assert.Equal(
            new Coordinate(90, 0).GetHashCode(options),
            new Coordinate(90, 150).GetHashCode(options)
        );

        // The two anti-meridian longitudes hash together.
        Assert.Equal(
            new Coordinate(4, 180).GetHashCode(options),
            new Coordinate(4, -180).GetHashCode(options)
        );
    }

    [Fact]
    public void Equality_PoleCoordinates()
    {
        Assert.True(
            new CoordinateZM(90, 0, 0, 0).Equals(
                new CoordinateZM(90, 180, 0, 0),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = true }
            )
        );
        Assert.False(
            new CoordinateZM(90, 0, 0, 0).Equals(
                new CoordinateZM(90, 180, 0, 0),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = false }
            )
        );
    }

    [Fact]
    public void Equality_SouthPoleCoordinates()
    {
        Assert.True(
            new Coordinate(-90, 0).Equals(
                new Coordinate(-90, 180),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = true }
            )
        );
        Assert.False(
            new Coordinate(-90, 0).Equals(
                new Coordinate(-90, 180),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = false }
            )
        );
    }

    [Fact]
    public void Equality_AntiMeridianCoordinates()
    {
        Assert.True(
            new Coordinate(4, 180).Equals(
                new Coordinate(4, -180),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = true }
            )
        );
        Assert.False(
            new Coordinate(4, 180).Equals(
                new Coordinate(4, -180),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = false }
            )
        );
    }

    [Fact]
    public void Parse_null_throws_argument_null_exception()
    {
        Assert.Throws<ArgumentNullException>(() => Coordinate.Parse(null));
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Parse_empty_or_whitespace_throws_argument_exception(string value)
    {
        Assert.Throws<ArgumentException>(() => Coordinate.Parse(value));
    }

    [Fact]
    public void Parse_unrecognised_format_throws_format_exception()
    {
        Assert.Throws<FormatException>(() => Coordinate.Parse("not a coordinate"));
    }

    [Fact]
    public void TryParse_returns_false_for_unrecognised_input()
    {
        var success = Coordinate.TryParse("not a coordinate", out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_returns_true_for_valid_input()
    {
        var success = Coordinate.TryParse("42.294498, -89.637901", out var result);

        Assert.True(success);
        Assert.NotNull(result);
        Assert.Equal(42.294498, result.Latitude);
        Assert.Equal(-89.637901, result.Longitude);
    }

    [Fact]
    public void TryParse_string_overload_returns_null_for_unrecognised_input()
    {
        Assert.Null(Coordinate.TryParse("not a coordinate"));
        Assert.NotNull(Coordinate.TryParse("1, 2"));
    }

    [Fact]
    public void TryParse_returns_false_for_null_input()
    {
        // TryParse reports failure rather than throwing, unlike Parse.
        var success = Coordinate.TryParse(null, out var result);

        Assert.False(success);
        Assert.Null(result);
        Assert.Null(Coordinate.TryParse(null));
    }

    [Theory]
    // The hemisphere letter written in front of the ordinate, as aviation and marine
    // sources overwhelmingly write it. None of these parsed at all before.
    [InlineData("N51 30.0, W000 07.2", 51.5, -0.12)]
    [InlineData("N51 30.0 W000 07.2", 51.5, -0.12)]
    [InlineData("N 51 30.0, W 000 07.2", 51.5, -0.12)]
    [InlineData("N51°30.0', W000°07.2'", 51.5, -0.12)]
    [InlineData("N51 30 00, W000 07 12", 51.5, -0.12)]
    [InlineData("S33 52 00, E151 12 00", -33.866666666666667, 151.2)]
    [InlineData("N51.5, W0.12", 51.5, -0.12)]
    [InlineData("n51.5, w0.12", 51.5, -0.12)]
    [InlineData("(N51 30.0, W000 07.2)", 51.5, -0.12)]
    // Stated on both sides, saying the same thing twice.
    [InlineData("N51 30.0N, W000 07.2W", 51.5, -0.12)]
    public void Parse_accepts_a_leading_hemisphere_letter(
        string coordinate,
        double latitude,
        double longitude
    )
    {
        var result = Coordinate.Parse(coordinate);

        Assert.Equal(latitude, result.Latitude, 10);
        Assert.Equal(longitude, result.Longitude, 10);
    }

    [Theory]
    // The two letters contradict each other.
    [InlineData("N51 30.0S, W000 07.2W")]
    [InlineData("N51 30.0, W000 07.2E")]
    // A letter naming the wrong axis. Ignoring it dropped the hemisphere silently and put
    // the position on the wrong side of the meridian, so it fails the parse instead; the
    // ordinates are read latitude first, and the caller has to present them that way.
    [InlineData("E51 30.0, W000 07.2")]
    [InlineData("N51 30.0, N000 07.2")]
    [InlineData("W000 07.2, N51 30.0")]
    [InlineData("0.12W, 51.5N")]
    public void TryParse_rejects_hemisphere_letters_it_cannot_honour(string coordinate)
    {
        Assert.False(Coordinate.TryParse(coordinate, out var result));
        Assert.Null(result);
    }

    [Theory]
    // The hyphen-separated form used by the FAA and the NGS. The hyphen is a separator,
    // not a sign: read as one, "40-26-46N" subtracted its own minutes and seconds and came
    // out 190 km from where it says, and "000-07-12W" drove the ordinate negative and then
    // let the W negate it again, landing on the wrong side of the meridian entirely.
    [InlineData("40-26-46N, 079-56-55W", 40.446111111111111, -79.948611111111111)]
    [InlineData("40-26-46N 079-56-55W", 40.446111111111111, -79.948611111111111)]
    [InlineData("51-30-00N, 000-07-12W", 51.5, -0.12)]
    [InlineData("N51-30-00, W000-07-12", 51.5, -0.12)]
    [InlineData("S33-52-00, E151-12-00", -33.866666666666667, 151.2)]
    [InlineData("51-30, 0-7", 51.5, 0.11666666666666667)]
    public void Parse_accepts_hyphen_separated_degrees_minutes_and_seconds(
        string coordinate,
        double latitude,
        double longitude
    )
    {
        var result = Coordinate.Parse(coordinate);

        Assert.Equal(latitude, result.Latitude, 10);
        Assert.Equal(longitude, result.Longitude, 10);
    }

    [Theory]
    // Only the degrees carry a sign. A minutes or seconds field is a magnitude - no
    // notation writes "51° -30'" - so the hyphen in front of one is the separator that
    // introduces it, and the field is added rather than subtracted.
    [InlineData("1 -2, 3", 1.0333333333333333, 3)]
    [InlineData("51 -30, 0", 51.5, 0)]
    // A sign on the degrees is still honoured, including where the ordinate that follows
    // is separated by whitespace alone.
    [InlineData("51.5 -0.12", 51.5, -0.12)]
    [InlineData("51.5, -0.12", 51.5, -0.12)]
    public void Minutes_and_seconds_are_magnitudes(
        string coordinate,
        double latitude,
        double longitude
    )
    {
        var result = Coordinate.Parse(coordinate);

        Assert.Equal(latitude, result.Latitude, 10);
        Assert.Equal(longitude, result.Longitude, 10);
    }

    [Theory]
    // A sign or a stray point glued to a number used to open a fresh minutes or seconds
    // field, so these returned a position instead of failing. Nothing writes a coordinate
    // this way, and requiring a separator is what makes the parse linear.
    [InlineData("1+2, 3")]
    [InlineData("4, 1+5")]
    [InlineData("1.2.3, 4")]
    [InlineData("1..2, 3")]
    public void TryParse_rejects_a_field_that_is_not_introduced_by_a_separator(string coordinate)
    {
        Assert.False(Coordinate.TryParse(coordinate, out var result));
        Assert.Null(result);
    }

    [Fact]
    public void TryParse_does_not_hang_on_a_long_run_of_digits()
    {
        // Six number fields whose separators were all optional used to let the engine carve
        // a digit run up an exponential number of ways before giving up: forty digits took
        // over five seconds, and each further digit multiplied that. Requiring a separator
        // in front of the minutes and the seconds leaves one way to read a run, so the
        // whole thing is linear and needs no match timeout to bound it.
        var stopwatch = Stopwatch.StartNew();

        Assert.False(Coordinate.TryParse(new string('1', 50000), out _));
        Assert.False(Coordinate.TryParse(new string('1', 50000) + "!", out _));
        Assert.False(Coordinate.TryParse("N" + new string('1', 50000), out _));
        Assert.False(Coordinate.TryParse(string.Concat(Enumerable.Repeat("1-", 50000)), out _));
        Assert.False(Coordinate.TryParse(string.Concat(Enumerable.Repeat("1-2, ", 50000)), out _));

        Assert.True(stopwatch.Elapsed < TimeSpan.FromSeconds(5));
    }

    [Theory]
    // "-0" parses to negative zero, which is not less than zero, so the sign has to come
    // from the text: every coordinate in the (-1, 0) degree band would otherwise come back
    // on the wrong side of the equator or the meridian.
    [InlineData("51 30 0, -0 7 12", 51.5, -0.12)]
    [InlineData("51 30 0, 0 7 12", 51.5, 0.12)]
    [InlineData("-0 30 0, 10 0 0", -0.5, 10)]
    [InlineData("-0 7.2, -0 7.2", -0.12, -0.12)]
    [InlineData("-0.5, -0.25", -0.5, -0.25)]
    // A hemisphere letter still wins over the sign of the degrees, as it always has.
    [InlineData("0 30 0 S, 0 7 12 W", -0.5, -0.12)]
    public void Parse_keeps_the_sign_of_a_negative_zero_degrees_field(
        string coordinate,
        double latitude,
        double longitude
    )
    {
        var result = Coordinate.Parse(coordinate);

        Assert.Equal(latitude, result.Latitude, 10);
        Assert.Equal(longitude, result.Longitude, 10);
    }

    [Theory]
    [InlineData("91, 0")]
    [InlineData("-91, 0")]
    [InlineData("0, 181")]
    [InlineData("0, -181")]
    public void TryParse_returns_false_for_out_of_range_ordinates(string coordinate)
    {
        var success = Coordinate.TryParse(coordinate, out var result);

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData("91, 0")]
    [InlineData("0, 181")]
    public void TryParse_string_overload_returns_null_for_out_of_range_ordinates(string coordinate)
    {
        Assert.Null(Coordinate.TryParse(coordinate));
    }

    [Theory]
    [InlineData(91, 0)]
    [InlineData(-91, 0)]
    [InlineData(0, 181)]
    [InlineData(0, -181)]
    public void Out_of_range_ordinates_throw(double latitude, double longitude)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(latitude, longitude));
    }

    [Fact]
    public void CoordinateZ_exposes_elevation_and_is_3d()
    {
        var coordinate = new CoordinateZ(1, 2, 3);

        Assert.Equal(3, coordinate.Elevation);
        Assert.True(coordinate.Is3D);
        Assert.False(coordinate.IsMeasured);
    }

    [Fact]
    public void CoordinateM_exposes_measure_and_is_measured()
    {
        var coordinate = new CoordinateM(1, 2, 3);

        Assert.Equal(3, coordinate.Measure);
        Assert.True(coordinate.IsMeasured);
        Assert.False(coordinate.Is3D);
    }

    [Fact]
    public void CoordinateZM_exposes_elevation_and_measure()
    {
        var coordinate = new CoordinateZM(1, 2, 3, 4);

        Assert.Equal(3, coordinate.Elevation);
        Assert.Equal(4, coordinate.Measure);
        Assert.True(coordinate.Is3D);
        Assert.True(coordinate.IsMeasured);
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void CoordinateZ_rejects_non_finite_elevation(double elevation)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CoordinateZ(1, 2, elevation));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void CoordinateM_rejects_non_finite_measure(double measure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CoordinateM(1, 2, measure));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void CoordinateZM_rejects_non_finite_elevation(double elevation)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CoordinateZM(1, 2, elevation, 5));
    }

    [Theory]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void CoordinateZM_rejects_non_finite_measure(double measure)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new CoordinateZM(1, 2, 3, measure));
    }

    [Fact]
    public void GetBounds_is_a_degenerate_envelope_at_the_coordinate()
    {
        Assert.Equal(new Envelope(12, 34, 12, 34), new Coordinate(12, 34).GetBounds());
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("en-US")]
    [InlineData("")]
    public void Parse_is_independent_of_the_current_culture(string culture)
    {
        // Cultures that swap the roles of '.' and ',' must not change how the degrees,
        // minutes and seconds of a coordinate are read.
        using (new CultureScope(culture))
        {
            var result = Coordinate.Parse("12 34.56'N 123 45.55'E");

            Assert.Equal(12.576, result.Latitude);
            Assert.Equal(123.75916666666667, result.Longitude);
        }
    }

    [Theory]
    [InlineData("de-DE")]
    [InlineData("fr-FR")]
    [InlineData("en-US")]
    [InlineData("")]
    public void ToString_round_trips_through_Parse_in_any_culture(string culture)
    {
        // A culture that writes the decimal separator as a comma would otherwise collide
        // with the comma separating the two ordinates.
        using (new CultureScope(culture))
        {
            var coordinate = new Coordinate(51.5, -0.12);

            var result = Coordinate.Parse(coordinate.ToString());

            Assert.Equal(coordinate.Latitude, result.Latitude);
            Assert.Equal(coordinate.Longitude, result.Longitude);
        }
    }

    private sealed class CultureScope : IDisposable
    {
        private readonly CultureInfo _original = CultureInfo.CurrentCulture;

        public CultureScope(string culture)
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
        }

        public void Dispose()
        {
            CultureInfo.CurrentCulture = _original;
        }
    }
}
