using System;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geometries;

public class LineStringTests
{
    [Fact]
    public void Default_line_string_is_empty_and_has_no_bounds()
    {
        var line = new LineString();

        Assert.True(line.IsEmpty);
        Assert.Null(line.GetBounds());
    }

    [Fact]
    public void GetBounds_spans_all_coordinates()
    {
        var line = new LineString(
            new Coordinate(0, 0),
            new Coordinate(10, -5),
            new Coordinate(-3, 8)
        );

        Assert.Equal(new Envelope(-3, -5, 10, 8), line.GetBounds());
    }

    [Fact]
    public void IsClosed_is_true_when_the_endpoints_coincide()
    {
        var open = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));
        var closed = new LineString(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            new Coordinate(0, 0)
        );

        Assert.False(open.IsClosed);
        Assert.True(closed.IsClosed);
    }

    [Fact]
    public void Is3D_and_IsMeasured_reflect_the_coordinates()
    {
        Assert.True(new LineString(new CoordinateZ(0, 0, 1), new CoordinateZ(1, 1, 2)).Is3D);
        Assert.True(new LineString(new CoordinateM(0, 0, 1), new CoordinateM(1, 1, 2)).IsMeasured);
    }

    [Fact]
    public void GetLength_is_positive_for_a_real_line()
    {
        var line = new LineString(new Coordinate(0, 0), new Coordinate(0, 1));

        Assert.True(line.GetLength().SiValue > 0);
    }

    [Fact]
    public void Indexer_returns_the_coordinate_at_the_position()
    {
        var line = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));

        Assert.Equal(new Coordinate(1, 1), line[1]);
    }

    [Fact]
    public void Equality_compares_the_coordinate_sequence()
    {
        var a = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));
        var b = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));
        var c = new LineString(new Coordinate(0, 0), new Coordinate(2, 2));

        Assert.True(a == b);
        Assert.True(a != c);
    }

    [Fact]
    public void LinearRing_requires_a_closed_sequence()
    {
        Assert.Throws<ArgumentException>(() =>
            new LinearRing(new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 0))
        );
    }

    [Fact]
    public void LinearRing_accepts_a_closed_sequence()
    {
        var ring = new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            new Coordinate(2, 0),
            new Coordinate(0, 0)
        );

        Assert.True(ring.IsClosed);
    }

    [Fact]
    public void An_empty_linear_ring_is_allowed()
    {
        var ring = new LinearRing();

        Assert.True(ring.IsEmpty);
    }

    [Fact]
    public void LinearRing_requires_at_least_four_coordinates()
    {
        // A closed sequence of three coordinates repeats the first point but does not
        // enclose an area; OGC/NTS require a ring to have zero or at least four points.
        Assert.Throws<ArgumentException>(() =>
            new LinearRing(new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(0, 0))
        );
    }

    [Fact]
    public void A_single_coordinate_line_string_is_not_closed()
    {
        var line = new LineString(new Coordinate(0, 0));

        Assert.False(line.IsClosed);
    }
}
