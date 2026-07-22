using Geo.Geometries;
using Geo.Linq;
using Xunit;

namespace Geo.Tests.Linq;

public class Spatial2DComparerTests
{
    private static readonly Spatial2DComparer<Point> Comparer = new();

    [Fact]
    public void Equal_horizontal_position_ignores_elevation()
    {
        var a = new Point(1, 2, 100);
        var b = new Point(1, 2, 200);

        Assert.True(Comparer.Equals(a, b));
        Assert.Equal(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void Different_horizontal_position_is_not_equal()
    {
        var a = new Point(1, 2, 100);
        var b = new Point(3, 4, 100);

        Assert.False(Comparer.Equals(a, b));
    }
}
