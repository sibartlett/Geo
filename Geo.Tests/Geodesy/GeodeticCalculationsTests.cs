using Geo.Geodesy;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geodesy;

public class GeodeticCalculationsTests
{
    [Fact]
    public void Meridional_parts_and_distance_are_zero_at_the_equator()
    {
        var equator = new Point(0, 0);

        Assert.Equal(0, equator.CalculateMeridionalParts(), 1e-6);
        Assert.Equal(0, equator.CalculateMeridionalDistance().SiValue, 1e-6);
    }

    [Fact]
    public void Meridional_distance_grows_with_latitude()
    {
        var mid = new Point(45, 0).CalculateMeridionalDistance().SiValue;
        var high = new Point(60, 0).CalculateMeridionalDistance().SiValue;

        Assert.True(mid > 0);
        Assert.True(high > mid);
    }

    [Fact]
    public void Meridional_calculations_are_symmetric_about_the_equator()
    {
        var north = new Point(30, 10).CalculateMeridionalDistance().SiValue;
        var south = new Point(-30, 10).CalculateMeridionalDistance().SiValue;

        Assert.Equal(north, -south, 1e-6);
    }

    [Fact]
    public void Shortest_line_and_great_circle_line_are_identical()
    {
        var a = new Point(51.5, -0.12);
        var b = new Point(40.0, -105.0);

        var shortest = a.CalculateShortestLine(b);
        var greatCircle = a.CalculateGreatCircleLine(b);

        Assert.Equal(shortest.Distance.SiValue, greatCircle.Distance.SiValue, 1e-6);
        Assert.Equal(shortest.Bearing12, greatCircle.Bearing12, 1e-6);
        Assert.Equal(shortest.Bearing21, greatCircle.Bearing21, 1e-6);
    }

    [Fact]
    public void Shortest_line_matches_the_underlying_calculator()
    {
        var a = new Point(51.5, -0.12);
        var b = new Point(48.85, 2.35);

        var viaExtension = a.CalculateShortestLine(b);
        var viaCalculator = GeoContext.Current.GeodeticCalculator.CalculateOrthodromicLine(a, b);

        Assert.Equal(viaCalculator.Distance.SiValue, viaExtension.Distance.SiValue, 1e-6);
    }

    [Fact]
    public void Along_a_meridian_the_rhumb_line_equals_the_shortest_line()
    {
        // North-south pairs share the meridian, so the loxodrome and orthodrome coincide.
        var a = new Point(10, 20);
        var b = new Point(50, 20);

        var rhumb = a.CalculateRhumbLine(b);
        var shortest = a.CalculateShortestLine(b);

        Assert.Equal(shortest.Distance.SiValue, rhumb.Distance.SiValue, 1e-3);
    }

    [Fact]
    public void Rhumb_line_differs_from_the_shortest_line_across_longitudes()
    {
        var a = new Point(60, -100);
        var b = new Point(60, 100);

        var rhumb = a.CalculateRhumbLine(b);
        var shortest = a.CalculateShortestLine(b);

        // The great-circle route across high latitudes is shorter than the constant-bearing route.
        Assert.True(shortest.Distance.SiValue < rhumb.Distance.SiValue);
    }
}
