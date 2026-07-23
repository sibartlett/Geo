using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Linq;
using Geo.Measure;
using Xunit;

namespace Geo.Tests.Linq;

public class EnumerableExtensionsTests
{
    [Fact]
    public void Distinct2D_collapses_coordinates_that_match_horizontally()
    {
        // Two coordinates at the same location but different elevations collapse
        // to one when compared in 2D.
        var coordinates = new List<Coordinate>
        {
            new CoordinateZ(0, 0, 100),
            new CoordinateZ(0, 0, 200),
            new CoordinateZ(1, 1, 0),
        };

        var distinct = coordinates.Distinct2D().ToList();

        Assert.Equal(2, distinct.Count);
    }

    [Fact]
    public void Distinct3D_keeps_coordinates_that_differ_in_elevation()
    {
        var coordinates = new List<Coordinate>
        {
            new CoordinateZ(0, 0, 100),
            new CoordinateZ(0, 0, 200),
            new CoordinateZ(0, 0, 100),
        };

        var distinct = coordinates.Distinct3D().ToList();

        Assert.Equal(2, distinct.Count);
    }

    [Fact]
    public void Sum_adds_distances_by_si_value()
    {
        var distances = new[] { new Distance(100), new Distance(250) };

        Assert.Equal(350, distances.Sum(x => x).SiValue);
    }

    [Fact]
    public void Sum_adds_areas_by_si_value()
    {
        var areas = new[] { new Area(100), new Area(250) };

        Assert.Equal(350, areas.Sum(x => x).SiValue);
    }

    [Fact]
    public void Max_and_Min_select_by_si_value()
    {
        var distances = new[] { new Distance(100), new Distance(250), new Distance(50) };

        Assert.Equal(250, distances.Max(x => x).SiValue);
        Assert.Equal(50, distances.Min(x => x).SiValue);
    }

    [Fact]
    public void Max_and_Min_work_for_areas()
    {
        var areas = new[] { new Area(100), new Area(250), new Area(50) };

        Assert.Equal(250, areas.Max(x => x).SiValue);
        Assert.Equal(50, areas.Min(x => x).SiValue);
    }

    [Fact]
    public void OrderCounterClockwise_orders_shuffled_box_corners_into_a_ring()
    {
        // The four corners of a box, deliberately out of order.
        var bottomLeft = new Coordinate(0, 0);
        var bottomRight = new Coordinate(0, 10);
        var topRight = new Coordinate(10, 10);
        var topLeft = new Coordinate(10, 0);

        var shuffled = new List<Coordinate> { topRight, bottomLeft, topLeft, bottomRight };

        var ordered = shuffled.OrderCounterClockwise();

        // Same four corners, now walking counter-clockwise, so the ring is valid.
        Assert.Equal(4, ordered.Count);
        Assert.Equal(WindingOrder.CounterClockwise, ordered.GetWindingOrder());
    }

    [Fact]
    public void OrderClockwise_and_OrderCounterClockwise_produce_opposite_windings()
    {
        var corners = new List<Coordinate> { new(10, 10), new(0, 0), new(10, 0), new(0, 10) };

        Assert.Equal(WindingOrder.Clockwise, corners.OrderClockwise().GetWindingOrder());
        Assert.Equal(
            WindingOrder.CounterClockwise,
            corners.OrderCounterClockwise().GetWindingOrder()
        );
    }

    [Fact]
    public void OrderCounterClockwise_closes_into_a_usable_linear_ring()
    {
        var shuffled = new List<Coordinate> { new(10, 10), new(0, 0), new(10, 0), new(0, 10) };

        var ordered = shuffled.OrderCounterClockwise().ToList();
        ordered.Add(ordered[0]); // close the ring

        var ring = new LinearRing(ordered);

        Assert.True(ring.IsClosed);
    }

    [Fact]
    public void OrderClockwise_is_a_permutation_of_the_input()
    {
        var corners = new List<Coordinate> { new(10, 10), new(0, 0), new(10, 0), new(0, 10) };

        var ordered = corners.OrderClockwise();

        Assert.Equal(corners.Count, ordered.Count);
        foreach (var corner in corners)
            Assert.Contains(corner, ordered);
    }

    [Fact]
    public void Ordering_fewer_than_three_coordinates_returns_them_unchanged()
    {
        var coordinates = new List<Coordinate> { new(1, 1), new(2, 2) };

        Assert.Equal(coordinates, coordinates.OrderClockwise());
        Assert.Equal(coordinates, coordinates.OrderCounterClockwise());
    }

    [Fact]
    public void Ordering_preserves_the_coordinate_subtype()
    {
        var coordinates = new List<Coordinate>
        {
            new CoordinateZ(10, 10, 5),
            new CoordinateZ(0, 0, 5),
            new CoordinateZ(10, 0, 5),
            new CoordinateZ(0, 10, 5),
        };

        Assert.All(coordinates.OrderCounterClockwise(), x => Assert.True(x.Is3D));
    }

    [Fact]
    public void GetWindingOrder_returns_null_for_degenerate_rings()
    {
        Assert.Null(new List<Coordinate> { new(0, 0), new(0, 10) }.GetWindingOrder());

        // Three colinear points enclose no area.
        var colinear = new List<Coordinate> { new(0, 0), new(0, 5), new(0, 10) };
        Assert.Null(colinear.GetWindingOrder());
    }

    [Fact]
    public void GetWindingOrder_ignores_a_repeated_closing_coordinate()
    {
        // With longitude as x and latitude as y, this ring sweeps counter-clockwise.
        var closed = new List<Coordinate>
        {
            new(0, 0),
            new(0, 10),
            new(10, 10),
            new(10, 0),
            new(0, 0),
        };

        Assert.Equal(WindingOrder.CounterClockwise, closed.GetWindingOrder());
    }

    [Fact]
    public void Ordering_and_winding_order_reject_null()
    {
        IEnumerable<Coordinate> coordinates = null!;

        Assert.Throws<ArgumentNullException>(() => coordinates.OrderClockwise());
        Assert.Throws<ArgumentNullException>(() => coordinates.OrderCounterClockwise());
        Assert.Throws<ArgumentNullException>(() => coordinates.GetWindingOrder());
    }
}
