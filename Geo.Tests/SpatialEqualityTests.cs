using Geo.Geometries;
using Xunit;

namespace Geo.Tests;

/// <summary>
/// Exercises the <see cref="SpatialEqualityOptions" /> permutations (pole, anti-meridian,
/// elevation and measure toggles) across the coordinate hierarchy and spatial collections.
/// These option-driven branches carry the library's subtlest equality logic and are otherwise
/// thinly covered.
/// </summary>
public class SpatialEqualityTests
{
    private static readonly SpatialEqualityOptions All = new();

    #region CoordinateZ

    [Fact]
    public void CoordinateZ_anti_meridian_coordinates_compare_equal_when_enabled()
    {
        Assert.True(
            new CoordinateZ(4, 180, 7).Equals(
                new CoordinateZ(4, -180, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = true }
            )
        );
        Assert.False(
            new CoordinateZ(4, 180, 7).Equals(
                new CoordinateZ(4, -180, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = false }
            )
        );
    }

    [Fact]
    public void CoordinateZ_pole_coordinates_compare_equal_when_enabled()
    {
        Assert.True(
            new CoordinateZ(-90, 0, 7).Equals(
                new CoordinateZ(-90, 150, 7),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = true }
            )
        );
        Assert.False(
            new CoordinateZ(-90, 0, 7).Equals(
                new CoordinateZ(-90, 150, 7),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = false }
            )
        );
    }

    [Fact]
    public void CoordinateZ_is_not_equal_to_null_or_other_dimensions()
    {
        var z = new CoordinateZ(1, 2, 3);

        Assert.False(z.Equals(null, All));
        Assert.False(z.Equals(new Coordinate(1, 2), All));
        Assert.False(z.Equals(new CoordinateM(1, 2, 3), All));
    }

    [Fact]
    public void CoordinateZ_hashcode_honours_pole_anti_meridian_and_elevation()
    {
        // Pole longitudes collapse together.
        Assert.Equal(
            new CoordinateZ(90, 0, 5).GetHashCode(All),
            new CoordinateZ(90, 150, 5).GetHashCode(All)
        );

        // Anti-meridian longitudes hash together.
        Assert.Equal(
            new CoordinateZ(4, 180, 5).GetHashCode(All),
            new CoordinateZ(4, -180, 5).GetHashCode(All)
        );

        // Elevation participates only when UseElevation is set.
        var without = new SpatialEqualityOptions { UseElevation = false };
        Assert.Equal(
            new CoordinateZ(1, 2, 100).GetHashCode(without),
            new CoordinateZ(1, 2, 200).GetHashCode(without)
        );
        Assert.NotEqual(
            new CoordinateZ(1, 2, 100).GetHashCode(All),
            new CoordinateZ(1, 2, 200).GetHashCode(All)
        );
    }

    #endregion

    #region CoordinateM

    [Fact]
    public void CoordinateM_anti_meridian_coordinates_compare_equal_when_enabled()
    {
        Assert.True(
            new CoordinateM(4, -180, 7).Equals(
                new CoordinateM(4, 180, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = true }
            )
        );
        Assert.False(
            new CoordinateM(4, -180, 7).Equals(
                new CoordinateM(4, 180, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = false }
            )
        );
    }

    [Fact]
    public void CoordinateM_pole_coordinates_compare_equal_when_enabled()
    {
        Assert.True(
            new CoordinateM(90, 0, 7).Equals(
                new CoordinateM(90, 150, 7),
                new SpatialEqualityOptions { PoleCoordiantesAreEqual = true }
            )
        );
    }

    [Fact]
    public void CoordinateM_is_not_equal_to_null_or_other_dimensions()
    {
        var m = new CoordinateM(1, 2, 3);

        Assert.False(m.Equals(null, All));
        Assert.False(m.Equals(new Coordinate(1, 2), All));
        Assert.False(m.Equals(new CoordinateZ(1, 2, 3), All));
    }

    [Fact]
    public void CoordinateM_hashcode_honours_pole_anti_meridian_and_measure()
    {
        Assert.Equal(
            new CoordinateM(90, 0, 5).GetHashCode(All),
            new CoordinateM(90, 150, 5).GetHashCode(All)
        );
        Assert.Equal(
            new CoordinateM(4, 180, 5).GetHashCode(All),
            new CoordinateM(4, -180, 5).GetHashCode(All)
        );

        var without = new SpatialEqualityOptions { UseM = false };
        Assert.Equal(
            new CoordinateM(1, 2, 100).GetHashCode(without),
            new CoordinateM(1, 2, 200).GetHashCode(without)
        );
        Assert.NotEqual(
            new CoordinateM(1, 2, 100).GetHashCode(All),
            new CoordinateM(1, 2, 200).GetHashCode(All)
        );
    }

    #endregion

    #region CoordinateZM

    [Fact]
    public void CoordinateZM_anti_meridian_coordinates_compare_equal_when_enabled()
    {
        Assert.True(
            new CoordinateZM(4, 180, 5, 7).Equals(
                new CoordinateZM(4, -180, 5, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = true }
            )
        );
        Assert.False(
            new CoordinateZM(4, 180, 5, 7).Equals(
                new CoordinateZM(4, -180, 5, 7),
                new SpatialEqualityOptions { AntiMeridianCoordinatesAreEqual = false }
            )
        );
    }

    [Fact]
    public void CoordinateZM_distinguishes_elevation_and_measure_independently()
    {
        var origin = new CoordinateZM(1, 2, 5, 7);

        // Elevation differs.
        Assert.False(origin.Equals(new CoordinateZM(1, 2, 99, 7), All));
        Assert.True(
            origin.Equals(
                new CoordinateZM(1, 2, 99, 7),
                new SpatialEqualityOptions { UseElevation = false }
            )
        );

        // Measure differs.
        Assert.False(origin.Equals(new CoordinateZM(1, 2, 5, 99), All));
        Assert.True(
            origin.Equals(
                new CoordinateZM(1, 2, 5, 99),
                new SpatialEqualityOptions { UseM = false }
            )
        );
    }

    [Fact]
    public void CoordinateZM_is_not_equal_to_null_or_other_dimensions()
    {
        var zm = new CoordinateZM(1, 2, 3, 4);

        Assert.False(zm.Equals(null, All));
        Assert.False(zm.Equals(new Coordinate(1, 2), All));
        Assert.False(zm.Equals(new CoordinateZ(1, 2, 3), All));
    }

    [Fact]
    public void CoordinateZM_hashcode_honours_every_option()
    {
        // Pole longitudes collapse together.
        Assert.Equal(
            new CoordinateZM(90, 0, 5, 7).GetHashCode(All),
            new CoordinateZM(90, 150, 5, 7).GetHashCode(All)
        );

        // Anti-meridian longitudes hash together.
        Assert.Equal(
            new CoordinateZM(4, 180, 5, 7).GetHashCode(All),
            new CoordinateZM(4, -180, 5, 7).GetHashCode(All)
        );

        // Elevation excluded when UseElevation is false.
        var withoutElevation = new SpatialEqualityOptions { UseElevation = false };
        Assert.Equal(
            new CoordinateZM(1, 2, 100, 7).GetHashCode(withoutElevation),
            new CoordinateZM(1, 2, 200, 7).GetHashCode(withoutElevation)
        );

        // Measure excluded when UseM is false.
        var withoutMeasure = new SpatialEqualityOptions { UseM = false };
        Assert.Equal(
            new CoordinateZM(1, 2, 5, 100).GetHashCode(withoutMeasure),
            new CoordinateZM(1, 2, 5, 200).GetHashCode(withoutMeasure)
        );

        // Both included by default.
        Assert.NotEqual(
            new CoordinateZM(1, 2, 5, 7).GetHashCode(All),
            new CoordinateZM(1, 2, 6, 7).GetHashCode(All)
        );
        Assert.NotEqual(
            new CoordinateZM(1, 2, 5, 7).GetHashCode(All),
            new CoordinateZM(1, 2, 5, 8).GetHashCode(All)
        );
    }

    #endregion

    #region Spatial collections

    [Fact]
    public void Collection_2d_equality_ignores_elevation_but_3d_equality_respects_it()
    {
        var a = new CoordinateSequence(new CoordinateZ(0, 0, 10), new CoordinateZ(1, 1, 20));
        var b = new CoordinateSequence(new CoordinateZ(0, 0, 99), new CoordinateZ(1, 1, 88));

        Assert.True(a.Equals2D(b));
        Assert.False(a.Equals3D(b));
    }

    [Fact]
    public void Collection_is_not_equal_to_null_or_different_length()
    {
        var a = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var shorter = new CoordinateSequence(new Coordinate(0, 0));

        Assert.False(a.Equals(null, All));
        Assert.False(a.Equals(shorter, All));
    }

    [Fact]
    public void Collection_equality_compares_elements_positionally()
    {
        var a = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var same = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var reordered = new CoordinateSequence(new Coordinate(1, 1), new Coordinate(0, 0));

        Assert.True(a.Equals(same, All));
        Assert.False(a.Equals(reordered, All));
    }

    [Fact]
    public void Collection_hashcode_matches_for_equal_sequences()
    {
        var a = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));
        var same = new CoordinateSequence(new Coordinate(0, 0), new Coordinate(1, 1));

        Assert.Equal(a.GetHashCode(All), same.GetHashCode(All));
        Assert.Equal(a.GetHashCode(), same.GetHashCode());
    }

    [Fact]
    public void MultiPoint_respects_elevation_only_in_3d_comparison()
    {
        var a = new MultiPoint(new Point(0, 0, 10), new Point(1, 1, 20));
        var b = new MultiPoint(new Point(0, 0, 99), new Point(1, 1, 88));

        Assert.True(a.Equals2D(b));
        Assert.False(a.Equals3D(b));
    }

    #endregion

    #region Envelope

    [Fact]
    public void Envelope_boxed_equality_covers_null_self_and_wrong_type()
    {
        var envelope = new Envelope(0, 0, 10, 10);

        Assert.False(envelope.Equals((object?)null));
        Assert.True(envelope.Equals((object)envelope));
        Assert.False(envelope.Equals((object)"not an envelope"));
        Assert.True(envelope.Equals((object)new Envelope(0, 0, 10, 10)));
        Assert.False(envelope.Equals((object)new Envelope(0, 0, 10, 11)));
    }

    [Fact]
    public void Envelope_operators_and_hashcode_agree_with_equality()
    {
        var a = new Envelope(1, 2, 3, 4);
        var b = new Envelope(1, 2, 3, 4);
        var c = new Envelope(1, 2, 3, 5);

        Assert.True(a == b);
        Assert.False(a != b);
        Assert.True(a != c);
        Assert.Equal(a.GetHashCode(), b.GetHashCode());
    }

    #endregion
}
