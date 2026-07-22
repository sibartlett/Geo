using System.Linq;
using Xunit;

namespace Geo.Tests;

public class CoordinateSequenceTests
{
    [Fact]
    public void Empty_sequence_reports_no_bounds()
    {
        var sequence = new CoordinateSequence();

        Assert.True(sequence.IsEmpty);
        Assert.Null(sequence.GetBounds());
    }

    [Fact]
    public void GetBounds_spans_the_minimum_and_maximum_ordinates()
    {
        var sequence = new CoordinateSequence(
            new Coordinate(10, 20),
            new Coordinate(-5, 40),
            new Coordinate(30, -15)
        );

        var bounds = sequence.GetBounds();

        Assert.Equal(new Envelope(-5, -15, 30, 40), bounds);
    }

    [Fact]
    public void HasElevation_is_true_when_any_coordinate_is_3d()
    {
        Assert.False(
            new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1)).HasElevation
        );
        Assert.True(
            new CoordinateSequence(new Coordinate(0, 0), new CoordinateZ(1, 1, 5)).HasElevation
        );
    }

    [Fact]
    public void HasM_is_true_when_any_coordinate_is_measured()
    {
        Assert.False(new CoordinateSequence(new Coordinate(0, 0)).HasM);
        Assert.True(new CoordinateSequence(new CoordinateM(0, 0, 3)).HasM);
    }

    [Fact]
    public void IsClosed_requires_more_than_two_matching_endpoints()
    {
        var closed = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            new Coordinate(2, 0),
            new Coordinate(0, 0)
        );
        var open = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            new Coordinate(2, 0)
        );

        Assert.True(closed.IsClosed);
        Assert.False(open.IsClosed);
    }

    [Fact]
    public void ToLineSegments_yields_one_segment_per_adjacent_pair()
    {
        var sequence = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(1, 1),
            new Coordinate(2, 2)
        );

        var segments = sequence.ToLineSegments().ToList();

        Assert.Equal(2, segments.Count);
        Assert.Equal(new Coordinate(0, 0), segments[0].Coordinate1);
        Assert.Equal(new Coordinate(1, 1), segments[0].Coordinate2);
        Assert.Equal(new Coordinate(1, 1), segments[1].Coordinate1);
        Assert.Equal(new Coordinate(2, 2), segments[1].Coordinate2);
    }

    [Fact]
    public void Equality_compares_the_coordinates_in_order()
    {
        var a = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var b = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var c = new CoordinateSequence(new Coordinate(1, 1), new Coordinate(0, 0));

        Assert.True(a == b);
        Assert.True(a != c);
        Assert.True(a.Equals(b));
        Assert.False(a.Equals(c));
    }
}
