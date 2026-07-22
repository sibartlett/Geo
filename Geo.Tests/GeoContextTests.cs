using System;
using Geo;
using Geo.Geodesy;
using Xunit;

namespace Geo.Tests;

public class GeoContextTests : IDisposable
{
    // Snapshot the ambient context and restore it after every test so a swapped
    // GeoContext.Current (or a toggled flag) never leaks into another test.
    private readonly GeoContext _original = GeoContext.Current;

    public void Dispose()
    {
        GeoContext.Current = _original;
    }

    [Fact]
    public void A_new_context_has_sensible_defaults()
    {
        var context = new GeoContext();

        Assert.Same(Spheroid.Default, context.Spheroid);
        Assert.NotNull(context.GeodeticCalculator);
        Assert.NotNull(context.GeomagnetismCalculator);
        Assert.NotNull(context.EqualityOptions);
        Assert.False(context.LongitudeWrapping);
    }

    [Fact]
    public void Current_lazily_initialises_and_is_replaceable()
    {
        GeoContext.Current = null;
        Assert.NotNull(GeoContext.Current);

        var replacement = new GeoContext();
        GeoContext.Current = replacement;
        Assert.Same(replacement, GeoContext.Current);
    }

    [Fact]
    public void Longitude_out_of_range_throws_when_wrapping_is_disabled()
    {
        GeoContext.Current = new GeoContext { LongitudeWrapping = false };

        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(0, 200));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(0, -200));
    }

    [Theory]
    [InlineData(200, -160)]
    [InlineData(-200, 160)]
    [InlineData(360, 0)]
    [InlineData(-360, 0)]
    [InlineData(540, 180)]
    [InlineData(-540, -180)]
    [InlineData(181, -179)]
    [InlineData(-181, 179)]
    public void Longitude_is_wrapped_into_range_when_wrapping_is_enabled(
        double longitude,
        double expected
    )
    {
        GeoContext.Current = new GeoContext { LongitudeWrapping = true };

        var coordinate = new Coordinate(0, longitude);

        Assert.Equal(expected, coordinate.Longitude);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(150)]
    [InlineData(-150)]
    [InlineData(180)]
    [InlineData(-180)]
    public void In_range_longitudes_are_untouched_by_wrapping(double longitude)
    {
        GeoContext.Current = new GeoContext { LongitudeWrapping = true };

        var coordinate = new Coordinate(0, longitude);

        Assert.Equal(longitude, coordinate.Longitude);
    }

    [Fact]
    public void Wrapping_does_not_suppress_out_of_range_latitude()
    {
        GeoContext.Current = new GeoContext { LongitudeWrapping = true };

        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(91, 200));
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(-91, 200));
    }

    [Fact]
    public void Coordinate_construction_reads_the_current_context()
    {
        GeoContext.Current = new GeoContext { LongitudeWrapping = false };
        Assert.Throws<ArgumentOutOfRangeException>(() => new Coordinate(0, 200));

        GeoContext.Current = new GeoContext { LongitudeWrapping = true };
        Assert.Equal(-160, new Coordinate(0, 200).Longitude);
    }
}
