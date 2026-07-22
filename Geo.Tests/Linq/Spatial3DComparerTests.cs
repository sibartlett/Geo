using Geo.Geometries;
using Geo.Linq;
using Xunit;

namespace Geo.Tests.Linq;

public class Spatial3DComparerTests
{
    private static readonly Spatial3DComparer<Point> Comparer = new();

    [Fact]
    public void Same_position_and_elevation_are_equal()
    {
        var a = new Point(1, 2, 100);
        var b = new Point(1, 2, 100);

        Assert.True(Comparer.Equals(a, b));
        Assert.Equal(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void Different_elevation_is_not_equal()
    {
        var a = new Point(1, 2, 100);
        var b = new Point(1, 2, 200);

        Assert.False(Comparer.Equals(a, b));
    }

    [Fact]
    public void Different_horizontal_position_is_not_equal()
    {
        var a = new Point(1, 2, 100);
        var b = new Point(3, 4, 100);

        Assert.False(Comparer.Equals(a, b));
    }
}
