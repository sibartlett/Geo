using System;
using System.Collections.Generic;
using Geo.Geodesy;
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

    [Fact]
    public void A_geodetic_line_is_not_equal_to_a_plain_segment_between_the_same_coordinates()
    {
        // A GeodeticLine also carries a distance and two bearings, and compares those, so
        // it was never equal to a segment; the segment, matching on anything assignable to
        // LineSegment, was equal to it. That asymmetry made List.Contains answer differently
        // depending on which of the two it happened to be holding, and left the pair with
        // different hash codes while still comparing equal one way round.
        var segment = new LineSegment(new Coordinate(0, 0), new Coordinate(1, 1));
        var line = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 5, 10, 20);

        Assert.False(segment.Equals(line));
        Assert.False(line.Equals(segment));

        Assert.DoesNotContain(line, new List<LineSegment> { segment });
        Assert.DoesNotContain(segment, new List<LineSegment> { line });
    }

    [Fact]
    public void Equality_still_holds_between_two_geodetic_lines()
    {
        var a = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 5, 10, 20);
        var same = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 5, 10, 20);
        var otherBearing = new GeodeticLine(new Coordinate(0, 0), new Coordinate(1, 1), 5, 99, 20);

        Assert.True(a.Equals(same));
        Assert.Equal(a.GetHashCode(), same.GetHashCode());
        Assert.False(a.Equals(otherBearing));
    }

    [Fact]
    public void Equality_rejects_null_and_unrelated_types()
    {
        var segment = new LineSegment(new Coordinate(0, 0), new Coordinate(1, 1));

        Assert.False(segment.Equals(null));
        Assert.False(segment.Equals("not a line segment"));
    }
}
