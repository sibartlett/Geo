using Geo.Measure;
using Xunit;

namespace Geo.Tests.Measure;

public class DistanceTests
{
    [Fact]
    public void Metres_constructor_stores_si_value()
    {
        var distance = new Distance(1234.5);

        Assert.Equal(1234.5, distance.SiValue);
        Assert.Equal(DistanceUnit.M, distance.Unit);
        Assert.Equal(1234.5, distance.Value);
    }

    [Theory]
    [InlineData(1, DistanceUnit.M, 1)]
    [InlineData(1, DistanceUnit.Km, 1000)]
    [InlineData(1, DistanceUnit.Nm, 1852)]
    [InlineData(1, DistanceUnit.Mile, 1609.34)]
    [InlineData(1, DistanceUnit.Ft, 0.3048)]
    public void Unit_constructor_converts_to_si_metres(
        double value,
        DistanceUnit unit,
        double expectedSiValue
    )
    {
        var distance = new Distance(value, unit);

        Assert.Equal(expectedSiValue, distance.SiValue, 1e-9);
        Assert.Equal(unit, distance.Unit);
        Assert.Equal(value, distance.Value, 1e-9);
    }

    [Theory]
    [InlineData(1000, DistanceUnit.Km, 1)]
    [InlineData(1852, DistanceUnit.Nm, 1)]
    [InlineData(1609.34, DistanceUnit.Mile, 1)]
    [InlineData(0.3048, DistanceUnit.Ft, 1)]
    public void ConvertTo_expresses_the_value_in_the_requested_unit(
        double metres,
        DistanceUnit unit,
        double expectedValue
    )
    {
        var converted = new Distance(metres).ConvertTo(unit);

        Assert.Equal(expectedValue, converted.Value, 1e-9);
        Assert.Equal(metres, converted.SiValue, 1e-9);
        Assert.Equal(unit, converted.Unit);
    }

    [Fact]
    public void ConvertTo_round_trips_back_to_metres()
    {
        var original = new Distance(5000);

        var roundTripped = original.ConvertTo(DistanceUnit.Mile).ConvertTo(DistanceUnit.M);

        Assert.Equal(original.SiValue, roundTripped.SiValue, 1e-6);
    }

    [Fact]
    public void GetConversionFactor_returns_the_units_metre_factor()
    {
        Assert.Equal(1, DistanceUnit.M.GetConversionFactor());
        Assert.Equal(1000, DistanceUnit.Km.GetConversionFactor());
        Assert.Equal(1852, DistanceUnit.Nm.GetConversionFactor());
    }

    [Fact]
    public void Double_extension_methods_convert_both_directions()
    {
        Assert.Equal(1, 1000d.ConvertTo(DistanceUnit.Km), 1e-9);
        Assert.Equal(1000, 1d.ConvertFrom(DistanceUnit.Km).To(DistanceUnit.M), 1e-9);
    }

    [Fact]
    public void Addition_and_subtraction_operate_on_si_values()
    {
        Assert.Equal(150, (new Distance(100) + new Distance(50)).SiValue);
        Assert.Equal(70, (new Distance(100) - new Distance(30)).SiValue);
    }

    [Fact]
    public void Multiplying_two_distances_produces_an_area()
    {
        Area area = new Distance(10) * new Distance(20);

        Assert.Equal(200, area.SiValue);
    }

    [Fact]
    public void Comparison_operators_reflect_magnitude()
    {
        var small = new Distance(50);
        var large = new Distance(100);

        Assert.True(large > small);
        Assert.True(small < large);
        Assert.True(large >= new Distance(100));
        Assert.True(small <= new Distance(50));
        Assert.Equal(1, large.CompareTo(small));
        Assert.Equal(-1, small.CompareTo(large));
        Assert.Equal(0, large.CompareTo(new Distance(100)));
    }

    [Fact]
    public void Equality_is_based_on_si_value()
    {
        Assert.True(new Distance(100) == new Distance(100));
        Assert.True(new Distance(100) != new Distance(200));
        Assert.True(new Distance(100).Equals(new Distance(100)));
        Assert.True(new Distance(100).Equals((object)new Distance(100)));
        Assert.False(new Distance(100).Equals((object)"not a distance"));
        Assert.Equal(new Distance(100).GetHashCode(), new Distance(100).GetHashCode());
    }

    [Fact]
    public void Explicit_conversion_from_numeric_types_yields_metres()
    {
        Assert.Equal(5, ((Distance)5).SiValue);
        Assert.Equal(5, ((Distance)5L).SiValue);
        Assert.Equal(5.5, ((Distance)5.5d).SiValue);
        Assert.Equal(5, ((Distance)5f).SiValue);
        Assert.Equal(5, ((Distance)5m).SiValue);
    }
}
