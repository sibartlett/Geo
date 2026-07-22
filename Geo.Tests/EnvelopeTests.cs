using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests;

public class EnvelopeTests
{
    [Fact]
    public void Constructor_exposes_the_bounds()
    {
        var envelope = new Envelope(-10, -20, 30, 40);

        Assert.Equal(-10, envelope.MinLat);
        Assert.Equal(-20, envelope.MinLon);
        Assert.Equal(30, envelope.MaxLat);
        Assert.Equal(40, envelope.MaxLon);
    }

    [Fact]
    public void Contains_coordinate_uses_strict_interior()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.True(envelope.Contains(new Coordinate(5, 5)));
        Assert.False(envelope.Contains(new Coordinate(0, 0)));
        Assert.False(envelope.Contains(new Coordinate(5, 10)));
        Assert.False(envelope.Contains(new Coordinate(20, 20)));
    }

    [Fact]
    public void Contains_envelope_requires_a_strictly_smaller_envelope()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.True(envelope.Contains(new Envelope(1, 1, 9, 9)));
        Assert.False(envelope.Contains(new Envelope(0, 0, 10, 10)));
        Assert.False(envelope.Contains(new Envelope(-1, -1, 11, 11)));
        Assert.False(envelope.Contains((Envelope)null));
    }

    [Fact]
    public void Contains_geometry_uses_the_geometry_bounds()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.True(envelope.Contains(new Point(5, 5)));
        Assert.False(envelope.Contains(new Point(50, 50)));
        Assert.False(envelope.Contains((IGeometry)null));
    }

    [Fact]
    public void Combine_produces_the_bounding_envelope_of_both()
    {
        var combined = new Envelope(0, 0, 5, 5).Combine(new Envelope(3, 3, 10, 12));

        Assert.Equal(new Envelope(0, 0, 10, 12), combined);
    }

    [Fact]
    public void Combine_with_null_returns_the_same_envelope()
    {
        var envelope = new Envelope(0, 0, 5, 5);

        Assert.Same(envelope, envelope.Combine(null));
    }

    [Fact]
    public void Intersects_is_true_for_overlapping_envelopes()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.True(envelope.Intersects(new Envelope(5, 5, 15, 15)));
        Assert.False(envelope.Intersects(new Envelope(20, 20, 30, 30)));
    }

    [Fact]
    public void Equality_compares_all_four_bounds()
    {
        var a = new Envelope(0, 0, 10, 10);
        var b = new Envelope(0, 0, 10, 10);
        var c = new Envelope(0, 0, 10, 11);

        Assert.True(a == b);
        Assert.True(a != c);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    [Fact]
    public void Area_and_length_are_positive_for_a_real_envelope()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.True(envelope.GetArea().SiValue > 0);
        Assert.True(envelope.GetLength().SiValue > 0);
    }

    [Fact]
    public void A_larger_envelope_has_a_larger_area()
    {
        var small = new Envelope(0, 0, 1, 1);
        var large = new Envelope(0, 0, 10, 10);

        Assert.True(large.GetArea().SiValue > small.GetArea().SiValue);
    }
}
