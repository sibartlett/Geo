using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geometries;

public class PointTests
{
    [Fact]
    public void Default_and_empty_points_are_empty()
    {
        Assert.True(new Point().IsEmpty);
        Assert.True(Point.Empty.IsEmpty);
        Assert.False(new Point(1, 2).IsEmpty);
    }

    [Fact]
    public void Two_argument_constructor_is_neither_3d_nor_measured()
    {
        var point = new Point(1, 2);

        Assert.False(point.Is3D);
        Assert.False(point.IsMeasured);
        Assert.IsType<Coordinate>(point.Coordinate);
    }

    [Fact]
    public void Elevation_constructor_is_3d()
    {
        var point = new Point(1, 2, 3);

        Assert.True(point.Is3D);
        Assert.False(point.IsMeasured);
        Assert.IsType<CoordinateZ>(point.Coordinate);
    }

    [Fact]
    public void Elevation_and_measure_constructor_is_3d_and_measured()
    {
        var point = new Point(1, 2, 3, 4);

        Assert.True(point.Is3D);
        Assert.True(point.IsMeasured);
        Assert.IsType<CoordinateZM>(point.Coordinate);
    }

    [Fact]
    public void GetBounds_is_a_degenerate_envelope_at_the_coordinate()
    {
        var bounds = new Point(12, 34).GetBounds();

        Assert.Equal(new Envelope(12, 34, 12, 34), bounds);
    }

    [Fact]
    public void Equality_compares_coordinates()
    {
        Assert.True(new Point(1, 2) == new Point(1, 2));
        Assert.True(new Point(1, 2) != new Point(1, 3));
        Assert.True(new Point(1, 2).Equals(new Point(1, 2)));
        Assert.Equal(new Point(1, 2).GetHashCode(), new Point(1, 2).GetHashCode());
    }

    [Fact]
    public void A_2d_point_does_not_equal_a_3d_point_at_the_same_location()
    {
        Assert.False(new Point(1, 2).Equals(new Point(1, 2, 3)));
        Assert.False(new Point(1, 2, 3).Equals(new Point(1, 2)));
    }

    [Fact]
    public void Two_empty_points_are_equal()
    {
        Assert.True(new Point().Equals(Point.Empty));
    }
}
