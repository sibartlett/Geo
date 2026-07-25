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

    // A counter-clockwise ring (the GeoJSON/OGC exterior-ring convention), which is
    // the opposite winding to Square above.
    private static LinearRing SquareCounterClockwise(double size)
    {
        return new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(0, size),
            new Coordinate(size, size),
            new Coordinate(size, 0),
            new Coordinate(0, 0)
        );
    }

    [Fact]
    public void GetArea_is_positive_regardless_of_shell_winding()
    {
        // Both windings describe the same square, so the area must be positive
        // and identical either way.
        var clockwise = new Polygon(Square(10)).GetArea().SiValue;
        var counterClockwise = new Polygon(SquareCounterClockwise(10)).GetArea().SiValue;

        Assert.True(counterClockwise > 0);
        Assert.Equal(clockwise, counterClockwise, clockwise * 1e-9);
    }

    [Fact]
    public void A_hole_reduces_the_area_of_a_standards_wound_polygon()
    {
        // GeoJSON/OGC winding: counter-clockwise shell, clockwise hole.
        var withoutHole = new Polygon(SquareCounterClockwise(10));
        var withHole = new Polygon(SquareCounterClockwise(10), Square(5));

        Assert.True(withHole.GetArea().SiValue > 0);
        Assert.True(withHole.GetArea().SiValue < withoutHole.GetArea().SiValue);
    }

    [Fact]
    public void Empty_polygons_are_equal()
    {
        Assert.True(new Polygon().Equals(Polygon.Empty));
    }

    [Fact]
    public void Empty_polygon_is_not_equal_to_a_non_empty_polygon()
    {
        var empty = new Polygon();
        var nonEmpty = new Polygon(Square(10));

        // The empty side has a null shell; comparing it must return false rather than
        // throwing, and the result must be symmetric.
        Assert.False(empty.Equals(nonEmpty));
        Assert.False(nonEmpty.Equals(empty));
        Assert.False(empty == nonEmpty);
        Assert.False(nonEmpty == empty);
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

    [Fact]
    public void Empty_polygon_has_zero_area()
    {
        // An empty polygon has a null shell; asking for its area must report that it
        // encloses nothing rather than dereferencing the missing shell.
        Assert.Equal(0d, new Polygon().GetArea().SiValue);
        Assert.Equal(0d, Polygon.Empty.GetArea().SiValue);
        Assert.Equal(0d, new Triangle().GetArea().SiValue);
    }

    [Fact]
    public void MultiPolygon_area_skips_empty_members()
    {
        var multiPolygon = new MultiPolygon(new Polygon(Square(10)), Polygon.Empty);

        Assert.Equal(new Polygon(Square(10)).GetArea().SiValue, multiPolygon.GetArea().SiValue);
    }

    [Fact]
    public void Dimensions_come_from_the_holes_as_well_as_the_shell()
    {
        // Regression: consulting the shell alone made a polygon whose elevations or
        // measures live in a hole describe itself as two-dimensional, which in turn
        // made the writers declare dimensions their coordinates did not carry.
        var flat = new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(0, 3),
            new Coordinate(3, 3),
            new Coordinate(0, 0)
        );
        var raised = new LinearRing(
            new CoordinateZ(1, 1, 7),
            new CoordinateZ(1, 2, 7),
            new CoordinateZ(2, 2, 7),
            new CoordinateZ(1, 1, 7)
        );
        var measured = new LinearRing(
            new CoordinateM(1, 1, 7),
            new CoordinateM(1, 2, 7),
            new CoordinateM(2, 2, 7),
            new CoordinateM(1, 1, 7)
        );

        Assert.True(new Polygon(flat, raised).Is3D);
        Assert.True(new Polygon(raised, flat).Is3D);
        Assert.False(new Polygon(flat, flat).Is3D);

        Assert.True(new Polygon(flat, measured).IsMeasured);
        Assert.True(new Polygon(measured, flat).IsMeasured);
        Assert.False(new Polygon(flat, flat).IsMeasured);

        // A collection sees through to them as well, since it reports the dimensions of
        // any member.
        Assert.True(new MultiPolygon(new Polygon(flat, raised)).Is3D);
    }

    [Fact]
    public void Empty_polygon_has_no_dimensions()
    {
        Assert.False(Polygon.Empty.Is3D);
        Assert.False(Polygon.Empty.IsMeasured);
    }
}
