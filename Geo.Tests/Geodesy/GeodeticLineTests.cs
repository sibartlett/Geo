using Geo.Geodesy;
using Geo.Measure;
using Xunit;

namespace Geo.Tests.Geodesy;

public class GeodeticLineTests
{
    private static GeodeticLine Line(
        double bearing12 = 45,
        double bearing21 = 225,
        double distance = 1000,
        double elevation = 0
    )
    {
        var c1 = new CoordinateZ(10, 20, elevation);
        var c2 = new CoordinateZ(30, 40, elevation);
        return new GeodeticLine(c1, c2, distance, bearing12, bearing21);
    }

    [Fact]
    public void Bearings_are_normalized_into_0_to_360()
    {
        var line = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 1000, 450, -10);

        Assert.Equal(90, line.Bearing12);
        Assert.Equal(350, line.Bearing21);
    }

    [Fact]
    public void Full_turn_bearing_normalizes_to_zero()
    {
        var line = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 1000, 360, 720);

        Assert.Equal(0, line.Bearing12);
        Assert.Equal(0, line.Bearing21);
    }

    [Fact]
    public void Stores_coordinates_and_distance()
    {
        var c1 = new Coordinate(10, 20);
        var c2 = new Coordinate(30, 40);
        var line = new GeodeticLine(c1, c2, 1234.5, 45, 225);

        Assert.Equal(c1, line.Coordinate1);
        Assert.Equal(c2, line.Coordinate2);
        Assert.Equal(1234.5, line.Distance.SiValue);
    }

    [Fact]
    public void Equal_lines_are_equal_and_share_a_hashcode()
    {
        var a = Line();
        var b = Line();

        Assert.True(a.Equals(b));
        Assert.True(a == b);
        Assert.False(a != b);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Theory]
    [InlineData(90, 225, 1000)] // different forward bearing
    [InlineData(45, 200, 1000)] // different reverse bearing
    [InlineData(45, 225, 2000)] // different distance
    public void Differs_when_a_component_differs(
        double bearing12,
        double bearing21,
        double distance
    )
    {
        var reference = Line();
        var other = Line(bearing12, bearing21, distance);

        Assert.False(reference.Equals(other));
        Assert.True(reference != other);
    }

    [Fact]
    public void Equality_operators_handle_null()
    {
        GeodeticLine line = Line();

        Assert.True((GeodeticLine)null == (GeodeticLine)null);
        Assert.False(line == null);
        Assert.False(null == line);
        Assert.True(line != null);
    }

    [Fact]
    public void Spatial_equality_can_ignore_elevation()
    {
        var seaLevel = Line(elevation: 0);
        var elevated = Line(elevation: 500);

        // Default comparison uses elevation, so the lines differ.
        Assert.False(seaLevel.Equals(elevated));

        // A 2D comparison ignores elevation, so they match.
        Assert.True(seaLevel.Equals(elevated, new SpatialEqualityOptions().To2D()));
        // A 3D comparison keeps elevation, so they still differ.
        Assert.False(seaLevel.Equals(elevated, new SpatialEqualityOptions().To3D()));
    }
}
