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
        Assert.True(new CoordinateZ(0, 0, 0).Equals(new CoordinateZ(0, 0, 0),
            new SpatialEqualityOptions { UseElevation = true }));
        Assert.False(new CoordinateZ(0, 0, 0).Equals(new CoordinateZ(0, 0, 10),
            new SpatialEqualityOptions { UseElevation = true }));
        Assert.True(new CoordinateZ(0, 0, 0).Equals(new CoordinateZ(0, 0, 10),
            new SpatialEqualityOptions { UseElevation = false }));
    }

    [Fact]
    public void Equality_M()
    {
        Assert.True(new CoordinateZM(0, 0, 0, 0).Equals(new CoordinateZM(0, 0, 0, 0),
            new SpatialEqualityOptions { UseM = true }));
        Assert.False(new CoordinateZM(0, 0, 0, 0).Equals(new CoordinateZM(0, 0, 0, 10),
            new SpatialEqualityOptions { UseM = true }));
        Assert.True(new CoordinateZM(0, 0, 0, 0).Equals(new CoordinateZM(0, 0, 0, 10),
            new SpatialEqualityOptions { UseM = false }));
    }

    [Fact]
    public void Equality_PoleCoordinates()
    {
        Assert.True(new CoordinateZM(90, 0, 0, 0).Equals(new CoordinateZM(90, 180, 0, 0),
            new SpatialEqualityOptions { PoleCoordiantesAreEqual = true }));
        Assert.False(new CoordinateZM(90, 0, 0, 0).Equals(new CoordinateZM(90, 180, 0, 0),
            new SpatialEqualityOptions { PoleCoordiantesAreEqual = false }));
    }

    [Fact]
    public void Equality_AntiMeridianCoordinates()
    {
        Assert.True(new Coordinate(4, 180).Equals(new Coordinate(4, -180),
            new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = true }));
        Assert.False(new Coordinate(4, 180).Equals(new Coordinate(4, -180),
            new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = false }));
    }
}