using System.Collections.Generic;
using System.Linq;
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
}
