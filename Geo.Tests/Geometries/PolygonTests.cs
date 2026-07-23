using System;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geometries;

public class PolygonTests
{
    // A counter-clockwise ring so the geodetic area comes out positive.
    private static LinearRing Square(double size)
    {
        return new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(size, 0),
            new Coordinate(size, size),
            new Coordinate(0, size),
            new Coordinate(0, 0)
        );
    }

    [Fact]
    public void Default_polygon_is_empty()
    {
        Assert.True(new Polygon().IsEmpty);
        Assert.True(Polygon.Empty.IsEmpty);
        Assert.False(new Polygon(Square(1)).IsEmpty);
    }

    [Fact]
    public void GetBounds_matches_the_shell_bounds()
    {
        var polygon = new Polygon(Square(10));

        Assert.Equal(polygon.Shell.GetBounds(), polygon.GetBounds());
    }

    [Fact]
    public void Empty_polygon_has_no_bounds()
    {
        Assert.Null(new Polygon().GetBounds());
        Assert.Null(Polygon.Empty.GetBounds());
    }

    [Fact]
    public void GetArea_is_positive_for_a_real_polygon()
    {
        var polygon = new Polygon(Square(1));

        Assert.True(polygon.GetArea().SiValue > 0);
    }

    [Fact]
    public void A_hole_reduces_the_polygon_area()
    {
        var withoutHole = new Polygon(Square(10));
        var withHole = new Polygon(Square(10), Square(5));

        Assert.True(withHole.GetArea().SiValue < withoutHole.GetArea().SiValue);
    }

    [Fact]
    public void Empty_polygons_are_equal()
    {
        Assert.True(new Polygon().Equals(Polygon.Empty));
    }

    [Fact]
    public void Equality_compares_shell_and_holes()
    {
        var a = new Polygon(Square(10));
        var b = new Polygon(Square(10));
        var c = new Polygon(Square(10), Square(5));

        Assert.True(a == b);
        Assert.True(a != c);
    }

    [Fact]
    public void Triangle_is_built_from_three_corners()
    {
        var triangle = new Triangle(
            new Coordinate(0, 0),
            new Coordinate(1, 0),
            new Coordinate(0, 1)
        );

        Assert.False(triangle.IsEmpty);
        Assert.True(triangle.GetArea().SiValue > 0);
        Assert.Equal(4, triangle.Shell.Coordinates.Count);
    }

    [Fact]
    public void Empty_triangle_is_empty()
    {
        Assert.True(new Triangle().IsEmpty);
        Assert.True(Triangle.Empty.IsEmpty);
    }
}
