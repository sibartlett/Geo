using System;
using Xunit;

namespace Geo.Tests;

public class LineSegmentTests
{
    [Fact]
    public void Constructor_exposes_both_coordinates()
    {
        var c1 = new Coordinate(0, 0);
        var c2 = new Coordinate(1, 1);

        var segment = new LineSegment(c1, c2);

        Assert.Same(c1, segment.Coordinate1);
        Assert.Same(c2, segment.Coordinate2);
    }

    [Fact]
    public void Null_coordinates_throw()
    {
        Assert.Throws<ArgumentNullException>(() => new LineSegment(null, new Coordinate(0, 0)));
        Assert.Throws<ArgumentNullException>(() => new LineSegment(new Coordinate(0, 0), null));
    }

    [Fact]
    public void GetBounds_spans_both_endpoints()
    {
        var segment = new LineSegment(new Coordinate(2, -3), new Coordinate(-4, 5));

        Assert.Equal(new Envelope(-4, -3, 2, 5), segment.GetBounds());
    }

    [Fact]
    public void Equality_compares_both_endpoints()
    {
        var a = new LineSegment(new Coordinate(0, 0), new Coordinate(1, 1));
        var b = new LineSegment(new Coordinate(0, 0), new Coordinate(1, 1));
        var reversed = new LineSegment(new Coordinate(1, 1), new Coordinate(0, 0));

        Assert.True(a == b);
        Assert.True(a != reversed);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(reversed));
    }
}
