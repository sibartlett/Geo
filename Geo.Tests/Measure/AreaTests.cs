using Geo.Measure;
using Xunit;

namespace Geo.Tests.Measure;

public class AreaTests
{
    [Fact]
    public void Metres_constructor_stores_si_value()
    {
        var area = new Area(2500);

        Assert.Equal(2500, area.SiValue);
    }

    [Fact]
    public void Addition_and_subtraction_operate_on_si_values()
    {
        Assert.Equal(300, (new Area(100) + new Area(200)).SiValue);
        Assert.Equal(150, (new Area(200) - new Area(50)).SiValue);
    }

    [Fact]
    public void Comparison_operators_reflect_magnitude()
    {
        var small = new Area(100);
        var large = new Area(400);

        Assert.True(large > small);
        Assert.True(small < large);
        Assert.True(large >= new Area(400));
        Assert.True(small <= new Area(100));
        Assert.Equal(1, large.CompareTo(small));
        Assert.Equal(0, large.CompareTo(new Area(400)));
    }

    [Fact]
    public void Equality_is_based_on_si_value()
    {
        Assert.True(new Area(100) == new Area(100));
        Assert.True(new Area(100) != new Area(200));
        Assert.True(new Area(100).Equals(new Area(100)));
        Assert.Equal(new Area(100).GetHashCode(), new Area(100).GetHashCode());
    }

    [Fact]
    public void Boxed_equality_compares_two_areas()
    {
        // Regression: Area.Equals(object) previously tested for Distance, so equal
        // areas compared as objects were reported unequal.
        object boxed = new Area(100);

        Assert.True(new Area(100).Equals(boxed));
        Assert.False(new Area(100).Equals((object)new Area(200)));
        Assert.False(new Area(100).Equals((object)new Distance(100)));
    }

    [Fact]
    public void Unit_constructor_converts_value_to_si_and_exposes_it_in_unit()
    {
        var area = new Area(2, DistanceUnit.Km);

        Assert.Equal(2000, area.SiValue);
        Assert.Equal(DistanceUnit.Km, area.Unit);
        Assert.Equal(2, area.Value, 1e-9);
    }

    [Fact]
    public void ConvertTo_expresses_the_value_in_the_requested_unit()
    {
        var converted = new Area(2000).ConvertTo(DistanceUnit.Km);

        Assert.Equal(2, converted.Value, 1e-9);
        Assert.Equal(DistanceUnit.Km, converted.Unit);
    }

    [Fact]
    public void ToString_formats_the_value_in_its_unit()
    {
        Assert.Equal(new Area(2000).ToString(), new Area(2000).ToString(DistanceUnit.M));
        Assert.Equal(
            new Area(2000).ConvertTo(DistanceUnit.Km).ToString(),
            new Area(2000).ToString(DistanceUnit.Km)
        );
    }

    [Fact]
    public void Explicit_conversions_from_numeric_types_store_square_metres()
    {
        Assert.Equal(5, ((Area)5).SiValue);
        Assert.Equal(5, ((Area)5L).SiValue);
        Assert.Equal(5, ((Area)5d).SiValue);
        Assert.Equal(5, ((Area)5f).SiValue);
        Assert.Equal(5, ((Area)5m).SiValue);
    }

    [Fact]
    public void Inclusive_comparison_operators_reflect_magnitude()
    {
        Assert.True(new Area(400) >= new Area(100));
        Assert.False(new Area(100) >= new Area(400));
        Assert.True(new Area(100) <= new Area(400));
        Assert.False(new Area(400) <= new Area(100));
        Assert.Equal(-1, new Area(100).CompareTo(new Area(400)));
    }

    [Theory]
    [InlineData(1_000_000, AreaUnit.Km, 1)]
    [InlineData(1, AreaUnit.M, 1)]
    public void Area_unit_extension_converts_from_square_metres(
        double squareMetres,
        AreaUnit unit,
        double expected
    )
    {
        Assert.Equal(expected, squareMetres.ConvertTo(unit), 1e-9);
    }

    [Fact]
    public void Area_unit_extension_round_trips()
    {
        Assert.Equal(1_000_000, 1d.ConvertFrom(AreaUnit.Km).To(AreaUnit.M), 1e-6);
    }

    [Fact]
    public void Area_unit_conversion_factor_is_the_square_of_the_linear_factor()
    {
        Assert.Equal(1000d * 1000d, AreaUnit.Km.GetConversionFactor());
        Assert.Equal(1852d * 1852d, AreaUnit.Nm.GetConversionFactor());
    }
}
