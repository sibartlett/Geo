using System;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geometries;

public class MultiGeometryTests
{
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
    public void MultiPoint_collects_its_points()
    {
        var multi = new MultiPoint(new Point(0, 0), new Point(1, 1));

        Assert.Equal(2, multi.Geometries.Count);
        Assert.False(multi.IsEmpty);
        Assert.True(new MultiPoint().IsEmpty);
    }

    [Fact]
    public void MultiLineString_length_is_the_sum_of_its_lines()
    {
        var a = new LineString(new Coordinate(0, 0), new Coordinate(0, 1));
        var b = new LineString(new Coordinate(0, 0), new Coordinate(0, 2));
        var multi = new MultiLineString(a, b);

        var expected = a.GetLength().SiValue + b.GetLength().SiValue;

        Assert.Equal(expected, multi.GetLength().SiValue, 1e-6);
    }

    [Fact]
    public void MultiPolygon_area_is_the_sum_of_its_polygons()
    {
        var a = new Polygon(Square(1));
        var b = new Polygon(Square(2));
        var multi = new MultiPolygon(a, b);

        var expected = a.GetArea().SiValue + b.GetArea().SiValue;

        Assert.Equal(expected, multi.GetArea().SiValue, 1e-6);
    }

    [Fact]
    public void GeometryCollection_bounds_combine_all_members()
    {
        var collection = new GeometryCollection(new Point(0, 0), new Point(10, 10));

        Assert.Equal(new Envelope(0, 0, 10, 10), collection.GetBounds());
    }

    [Fact]
    public void GeometryCollection_is_3d_when_any_member_is_3d()
    {
        var collection = new GeometryCollection(new Point(0, 0), new Point(1, 1, 5));

        Assert.True(collection.Is3D);
        Assert.False(new GeometryCollection(new Point(0, 0)).Is3D);
    }

    [Fact]
    public void Empty_geometry_collection_reports_empty()
    {
        Assert.True(new GeometryCollection().IsEmpty);
        Assert.Null(new GeometryCollection().GetBounds());
    }

    [Fact]
    public void GeometryCollection_equality_compares_members_in_order()
    {
        var a = new GeometryCollection(new Point(0, 0), new Point(1, 1));
        var b = new GeometryCollection(new Point(0, 0), new Point(1, 1));
        var c = new GeometryCollection(new Point(1, 1), new Point(0, 0));

        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }

    [Fact]
    public void GeometryCollection_is_not_equal_to_null_or_a_different_count()
    {
        var a = new GeometryCollection(new Point(0, 0), new Point(1, 1));
        var shorter = new GeometryCollection(new Point(0, 0));

        Assert.False(a.Equals(null));
        Assert.False(a.Equals("not a collection"));
        Assert.False(a.Equals(shorter));
    }

    [Fact]
    public void GeometryCollection_hashcode_matches_for_equal_collections()
    {
        var a = new GeometryCollection(new Point(0, 0), new Point(1, 1));
        var b = new GeometryCollection(new Point(0, 0), new Point(1, 1));

        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void LinearRing_must_be_closed()
    {
        // A non-empty ring whose last coordinate does not repeat the first is invalid.
        Assert.Throws<ArgumentException>(() =>
            new LinearRing(new Coordinate(0, 0), new Coordinate(1, 0), new Coordinate(0, 1))
        );
    }

    [Fact]
    public void Triangle_rejects_a_degenerate_shell()
    {
        Assert.Throws<ArgumentException>(() => new Triangle(new LinearRing()));
    }

    [Fact]
    public void Triangle_from_three_points_closes_the_ring()
    {
        var triangle = new Triangle(
            new Coordinate(0, 0),
            new Coordinate(1, 0),
            new Coordinate(0, 1)
        );

        Assert.True(triangle.Shell.IsClosed);
        Assert.Equal(4, triangle.Shell.Coordinates.Count);
    }
}
