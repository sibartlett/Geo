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
    public void A_zero_x_or_y_component_is_still_a_field(double x, double y)
    {
        const double z = 45000;

        var result = new GeomagnetismResult(Location, Date, x, y, z);

        var expectedH = Math.Sqrt(x * x + y * y);

        Assert.Equal(x, result.X);
        Assert.Equal(y, result.Y);
        Assert.Equal(z, result.Z);
        Assert.Equal(expectedH, result.HorizontalIntensity, 6);
        Assert.Equal(Math.Sqrt(x * x + y * y + z * z), result.TotalIntensity, 6);
        Assert.Equal(ToDegrees(Math.Atan2(y, x)), result.Declination, 9);
        Assert.Equal(ToDegrees(Math.Atan2(z, expectedH)), result.Inclination, 9);
        Assert.Same(Location, result.Coordinate);
        Assert.Equal(Date, result.Date);
    }

    [Fact]
    public void A_field_pointing_straight_down_keeps_its_vertical_component()
    {
        var result = new GeomagnetismResult(Location, Date, 0, 0, 45000);

        Assert.Equal(45000, result.Z);
        Assert.Equal(45000, result.TotalIntensity, 6);
        Assert.Equal(0, result.HorizontalIntensity, 6);
        Assert.Equal(90, result.Inclination, 9);
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
