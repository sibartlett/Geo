using System;
using Geo.Geomagnetism;
using Xunit;

namespace Geo.Tests.Geomagnetism;

public class GeomagnetismResultTests
{
    private static readonly DateTime Date = new(2012, 6, 15, 0, 0, 0, DateTimeKind.Utc);
    private static readonly CoordinateZ Location = new(51.5, -0.1, 0);

    private static double ToDegrees(double radians) => radians * 180 / Math.PI;

    [Fact]
    public void Derives_intensities_declination_and_inclination_from_the_field_vector()
    {
        const double x = 19000;
        const double y = -500;
        const double z = 45000;

        var result = new GeomagnetismResult(Location, Date, x, y, z);

        var expectedH = Math.Sqrt(x * x + y * y);
        var expectedF = Math.Sqrt(x * x + y * y + z * z);

        Assert.Equal(x, result.X);
        Assert.Equal(y, result.Y);
        Assert.Equal(z, result.Z);
        Assert.Equal(expectedH, result.HorizontalIntensity, 6);
        Assert.Equal(expectedF, result.TotalIntensity, 6);
        Assert.Equal(ToDegrees(Math.Atan2(y, x)), result.Declination, 9);
        Assert.Equal(ToDegrees(Math.Atan2(z, expectedH)), result.Inclination, 9);
        Assert.Same(Location, result.Coordinate);
        Assert.Equal(Date, result.Date);
    }

    [Theory]
    [InlineData(0, 100)]
    [InlineData(19000, 0)]
    public void A_zero_x_or_y_component_leaves_the_derived_field_unset(double x, double y)
    {
        var result = new GeomagnetismResult(Location, Date, x, y, 45000);

        // The constructor short-circuits before assigning any component, so every
        // derived quantity stays at its default of zero, while the location and date
        // are still recorded.
        Assert.Equal(0, result.X);
        Assert.Equal(0, result.Y);
        Assert.Equal(0, result.Z);
        Assert.Equal(0, result.HorizontalIntensity);
        Assert.Equal(0, result.TotalIntensity);
        Assert.Equal(0, result.Declination);
        Assert.Equal(0, result.Inclination);
        Assert.Same(Location, result.Coordinate);
        Assert.Equal(Date, result.Date);
    }

    [Fact]
    public void ToString_reports_every_component()
    {
        var text = new GeomagnetismResult(Location, Date, 19000, -500, 45000).ToString();

        Assert.StartsWith("Magnetic Field[", text);
        Assert.Contains("D=", text);
        Assert.Contains("I=", text);
        Assert.Contains("H=", text);
        Assert.Contains("F=", text);
    }
}
