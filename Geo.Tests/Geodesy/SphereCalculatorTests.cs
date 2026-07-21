using System;
using Geo.Geodesy;
using Xunit;

namespace Geo.Tests.Geodesy;

public class SphereCalculatorTests
{
    // Earth mean radius (matches Geo.Constants.EarthMeanRadius, which is internal).
    private const double Radius = 6371008.7714d;

    [Fact]
    public void Area_of_the_whole_sphere_equals_four_pi_r_squared()
    {
        var calculator = new SphereCalculator(Radius);
        var envelope = new Envelope(-90, -180, 90, 180);

        var expected = 4 * Math.PI * Radius * Radius;
        Assert.Equal(expected, calculator.CalculateArea(envelope).SiValue, expected * 1e-9);
    }

    [Fact]
    public void Area_of_the_northern_hemisphere_equals_two_pi_r_squared()
    {
        var calculator = new SphereCalculator(Radius);
        var envelope = new Envelope(0, -180, 90, 180);

        var expected = 2 * Math.PI * Radius * Radius;
        Assert.Equal(expected, calculator.CalculateArea(envelope).SiValue, expected * 1e-9);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 12363718034.176485)]
    [InlineData(0, 0, 10, 10, 1230166804525.028)]
    public void Area_of_an_envelope_is_positive_and_matches_the_spherical_zone(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        double expected
    )
    {
        var calculator = new SphereCalculator(Radius);
        var result = calculator.CalculateArea(new Envelope(minLat, minLon, maxLat, maxLon));

        Assert.True(result.SiValue > 0);
        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 444763.38338824594)]
    [InlineData(0, 0, 10, 10, 4430910.158223232)]
    public void Length_of_an_envelope_is_its_perimeter(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        double expected
    )
    {
        var calculator = new SphereCalculator(Radius);
        var result = calculator.CalculateLength(new Envelope(minLat, minLon, maxLat, maxLon));

        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }
}
