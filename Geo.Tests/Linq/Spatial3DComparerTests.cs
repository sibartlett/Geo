using System.Collections.Generic;
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

    [Fact]
    public void Elevation_keeps_stacked_positions_in_separate_buckets()
    {
        // What the options overload is for: without the elevation in the hash, every
        // coordinate sharing one position lands in the same bucket and Distinct3D
        // degrades to comparing each against all the rest.
        var a = new Point(1, 2, 100);
        var b = new Point(1, 2, 200);

        Assert.NotEqual(Comparer.GetHashCode(a), Comparer.GetHashCode(b));
    }

    [Fact]
    public void GetHashCode_does_not_depend_on_the_ambient_options()
    {
        var point = new Point(1, 2, 100);
        var previous = GeoContext.Current.EqualityOptions;
        try
        {
            var hashes = new HashSet<int>();
            foreach (var poles in new[] { true, false })
            foreach (var antiMeridian in new[] { true, false })
            {
                GeoContext.Current.EqualityOptions = new SpatialEqualityOptions
                {
                    PoleCoordiantesAreEqual = poles,
                    AntiMeridianCoordinatesAreEqual = antiMeridian,
                };

                hashes.Add(Comparer.GetHashCode(point));
            }

            Assert.Single(hashes);
        }
        finally
        {
            GeoContext.Current.EqualityOptions = previous;
        }
    }
}
