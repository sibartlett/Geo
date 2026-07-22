using System;
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
}
