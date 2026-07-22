using System;
using Geo.Measure;
using Xunit;

namespace Geo.Tests.Measure;

public class SpeedTests
{
    [Fact]
    public void Metres_per_second_constructor_stores_si_value()
    {
        var speed = new Speed(12.5);

        Assert.Equal(12.5, speed.SiValue);
        Assert.Equal(SpeedUnit.Ms, speed.Unit);
    }

    [Theory]
    [InlineData(1, SpeedUnit.Ms, 1)]
    [InlineData(1, SpeedUnit.Knots, 0.514444444)]
    [InlineData(1, SpeedUnit.Kph, 0.277778)]
    [InlineData(1, SpeedUnit.Mph, 0.44704)]
    public void Unit_constructor_converts_to_si_metres_per_second(
        double value,
        SpeedUnit unit,
        double expectedSiValue
    )
    {
        var speed = new Speed(value, unit);

        Assert.Equal(expectedSiValue, speed.SiValue, 1e-9);
        Assert.Equal(value, speed.Value, 1e-6);
    }

    [Fact]
    public void Distance_over_time_constructor_divides_metres_by_seconds()
    {
        var speed = new Speed(100, TimeSpan.FromSeconds(10));

        Assert.Equal(10, speed.SiValue);
    }

    [Theory]
    [InlineData(0)]
    public void Zero_distance_yields_zero_speed(double metres)
    {
        var speed = new Speed(metres, TimeSpan.FromSeconds(10));

        Assert.Equal(0, speed.SiValue);
    }

    [Fact]
    public void Zero_timespan_yields_zero_speed_instead_of_dividing_by_zero()
    {
        var speed = new Speed(100, TimeSpan.Zero);

        Assert.Equal(0, speed.SiValue);
    }

    [Fact]
    public void ConvertTo_expresses_value_in_requested_unit()
    {
        var converted = new Speed(1).ConvertTo(SpeedUnit.Kph);

        // 1 m/s is roughly 3.6 km/h.
        Assert.Equal(3.6, converted.Value, 1e-2);
    }

    [Fact]
    public void Addition_and_subtraction_operate_on_si_values()
    {
        Assert.Equal(30, (new Speed(10) + new Speed(20)).SiValue);
        Assert.Equal(5, (new Speed(20) - new Speed(15)).SiValue);
    }

    [Fact]
    public void Comparison_and_equality()
    {
        Assert.True(new Speed(20) > new Speed(10));
        Assert.True(new Speed(10) < new Speed(20));
        Assert.True(new Speed(10) == new Speed(10));
        Assert.True(new Speed(10) != new Speed(20));
        Assert.True(new Speed(10).Equals((object)new Speed(10)));
        Assert.Equal(0, new Speed(10).CompareTo(new Speed(10)));
    }
}
