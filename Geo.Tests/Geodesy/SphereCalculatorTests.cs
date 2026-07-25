using System;
using Geo.Geodesy;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geodesy;

public class SphereCalculatorTests
{
    // Earth mean radius (matches Geo.Constants.EarthMeanRadius, which is internal).
    private const double Radius = 6371008.7714d;

    [Fact]
    public void Area_of_the_whole_sphere_equals_four_pi_r_squared()
    {
        var calculator = new SphereCalculator(Radius);
        var envelope = new Envelope(-90, -180, 90, 180);

        var expected = 4 * Math.PI * Radius * Radius;
        Assert.Equal(expected, calculator.CalculateArea(envelope).SiValue, expected * 1e-9);
    }

    [Fact]
    public void Area_of_the_northern_hemisphere_equals_two_pi_r_squared()
    {
        var calculator = new SphereCalculator(Radius);
        var envelope = new Envelope(0, -180, 90, 180);

        var expected = 2 * Math.PI * Radius * Radius;
        Assert.Equal(expected, calculator.CalculateArea(envelope).SiValue, expected * 1e-9);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 12363718034.176485)]
    [InlineData(0, 0, 10, 10, 1230166804525.028)]
    public void Area_of_an_envelope_is_positive_and_matches_the_spherical_zone(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        double expected
    )
    {
        var calculator = new SphereCalculator(Radius);
        var result = calculator.CalculateArea(new Envelope(minLat, minLon, maxLat, maxLon));

        Assert.True(result.SiValue > 0);
        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }

    [Fact]
    public void Circle_area_of_a_cap_is_the_spherical_cap_area()
    {
        var calculator = new SphereCalculator(Radius);

        // Spherical cap area = 2 * pi * R * h, with h = R * (1 - cos(r / R)).
        var capRadius = 100000d;
        var h = Radius * (1 - Math.Cos(capRadius / Radius));
        var expected = 2 * Math.PI * Radius * h;

        var result = calculator.CalculateArea(new Circle(0, 0, capRadius));

        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }

    [Fact]
    public void Circle_area_is_zero_when_radius_is_not_positive()
    {
        var calculator = new SphereCalculator(Radius);

        Assert.Equal(0d, calculator.CalculateArea(new Circle(0, 0, 0)).SiValue);
        Assert.Equal(0d, calculator.CalculateArea(new Circle(0, 0, -1)).SiValue);
    }

    [Fact]
    public void Circle_area_is_zero_when_radius_exceeds_half_the_great_circle()
    {
        var calculator = new SphereCalculator(Radius);

        // Any radius beyond pi * R covers more than the whole sphere, so it is rejected.
        var tooLarge = Math.PI * Radius + 1;

        Assert.Equal(0d, calculator.CalculateArea(new Circle(0, 0, tooLarge)).SiValue);
    }

    [Fact]
    public void Circle_length_is_the_circumference_of_the_cap()
    {
        var calculator = new SphereCalculator(Radius);

        var capRadius = 100000d;
        var h = Radius * (1 - Math.Cos(capRadius / Radius));
        var expected = 2 * Math.PI * Math.Sqrt(h * (2 * Radius - h));

        var result = calculator.CalculateLength(new Circle(0, 0, capRadius));

        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }

    [Fact]
    public void Ring_area_is_a_positive_magnitude_regardless_of_winding_order()
    {
        var calculator = new SphereCalculator(Radius);

        // Same square traversed counter-clockwise (the GeoJSON/OGC exterior-ring
        // convention) and clockwise. Area is a magnitude, so both must be positive
        // and equal.
        var counterClockwise = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(0, 10),
            new Coordinate(10, 10),
            new Coordinate(10, 0),
            new Coordinate(0, 0)
        );
        var clockwise = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(10, 0),
            new Coordinate(10, 10),
            new Coordinate(0, 10),
            new Coordinate(0, 0)
        );

        var ccw = calculator.CalculateArea(counterClockwise).SiValue;
        var cw = calculator.CalculateArea(clockwise).SiValue;

        Assert.True(ccw > 0);
        Assert.True(cw > 0);
        Assert.Equal(cw, ccw, ccw * 1e-9);
    }

    [Theory]
    [InlineData(0, 0, 1, 1, 444763.38338824594)]
    [InlineData(0, 0, 10, 10, 4430910.158223232)]
    public void Length_of_an_envelope_is_its_perimeter(
        double minLat,
        double minLon,
        double maxLat,
        double maxLon,
        double expected
    )
    {
        var calculator = new SphereCalculator(Radius);
        var result = calculator.CalculateLength(new Envelope(minLat, minLon, maxLat, maxLon));

        Assert.Equal(expected, result.SiValue, expected * 1e-9);
    }

    #region Orthodromic (great circle) lines

    private static readonly SphereCalculator Calculator = new(Radius);

    /// <summary>The signed difference between two bearings, in degrees.</summary>
    private static double BearingDelta(double a, double b) => (a - b + 540) % 360 - 180;

    [Theory]
    // A quarter of the equator, and the equator to the pole: both a quarter great circle.
    [InlineData(0, 0, 0, 90)]
    [InlineData(0, 0, 90, 0)]
    public void Orthodromic_quarter_turns_are_a_quarter_of_a_great_circle(
        double lat1,
        double lon1,
        double lat2,
        double lon2
    )
    {
        var line = Calculator.CalculateOrthodromicLine(
            new Coordinate(lat1, lon1),
            new Coordinate(lat2, lon2)
        );

        var expected = Math.PI * Radius / 2;
        Assert.Equal(expected, line.Distance.SiValue, expected * 1e-12);
    }

    [Theory]
    [InlineData(0, 0, 0, 10, 90, 270)] // due east along the equator
    [InlineData(0, 10, 0, 0, 270, 90)] // due west along the equator
    [InlineData(0, 0, 10, 0, 0, 180)] // due north along a meridian
    [InlineData(10, 0, 0, 0, 180, 0)] // due south along a meridian
    public void Orthodromic_bearings_are_degrees_from_north(
        double lat1,
        double lon1,
        double lat2,
        double lon2,
        double expectedForward,
        double expectedBack
    )
    {
        var line = Calculator.CalculateOrthodromicLine(
            new Coordinate(lat1, lon1),
            new Coordinate(lat2, lon2)
        );

        Assert.Equal(0, BearingDelta(line.Bearing12, expectedForward), 1e-9);
        Assert.Equal(0, BearingDelta(line.Bearing21, expectedBack), 1e-9);
    }

    [Theory]
    [InlineData(-60)]
    [InlineData(0)]
    [InlineData(51.5)]
    [InlineData(80)]
    public void Orthodromic_direct_and_inverse_are_inverses_of_each_other(double latitude)
    {
        foreach (var heading in new[] { 0d, 37, 90, 143, 180, 231, 270, 322 })
        foreach (var distance in new[] { 1000d, 100000, 3000000 })
        {
            var start = new Coordinate(latitude, 10);
            var direct = Calculator.CalculateOrthodromicLine(start, heading, distance);
            var inverse = Calculator.CalculateOrthodromicLine(start, direct.Coordinate2);

            Assert.NotNull(inverse);
            Assert.Equal(distance, inverse.Distance.SiValue, distance * 1e-9);
            Assert.Equal(0, BearingDelta(inverse.Bearing12, direct.Bearing12), 1e-6);
            Assert.Equal(0, BearingDelta(inverse.Bearing21, direct.Bearing21), 1e-6);
        }
    }

    [Fact]
    public void Orthodromic_direct_solution_carries_on_over_the_pole()
    {
        // 2000 km north of 80N is about 18 degrees away, so it runs over the pole and
        // back down the far side: the latitude folds back to 180 - (80 + 18) on the
        // opposite meridian, and the way home is due north again.
        const double distance = 2000000;
        var line = Calculator.CalculateOrthodromicLine(new Coordinate(80, 0), 0, distance);

        var expected = 180 - (80 + distance / Radius * 180 / Math.PI);

        Assert.Equal(expected, line.Coordinate2.Latitude, 1e-9);
        Assert.Equal(180, Math.Abs(line.Coordinate2.Longitude), 1e-9);
        Assert.Equal(0, BearingDelta(line.Bearing21, 0), 1e-9);
    }

    [Fact]
    public void Orthodromic_direct_solution_wraps_across_the_anti_meridian()
    {
        const double distance = 500000;
        var line = Calculator.CalculateOrthodromicLine(new Coordinate(0, 179), 90, distance);

        // 500 km east of 179E is past the anti-meridian, so it comes back as a western
        // longitude rather than one Coordinate would refuse to hold.
        var expected = 179 + distance / Radius * 180 / Math.PI - 360;

        Assert.InRange(line.Coordinate2.Longitude, -180, 180);
        Assert.Equal(expected, line.Coordinate2.Longitude, 1e-9);
    }

    [Fact]
    public void Orthodromic_direct_solution_lands_exactly_on_the_pole()
    {
        var line = Calculator.CalculateOrthodromicLine(
            new Coordinate(0, 0),
            0,
            Math.PI * Radius / 2
        );

        Assert.Equal(90, line.Coordinate2.Latitude);
    }

    [Fact]
    public void Orthodromic_line_between_coincident_points_is_null()
    {
        Assert.Null(
            Calculator.CalculateOrthodromicLine(new Coordinate(1, 2), new Coordinate(1, 2))
        );
    }

    #endregion

    #region Loxodromic (rhumb) lines

    [Fact]
    public void Loxodromic_line_along_a_parallel_follows_the_parallel()
    {
        var line = Calculator.CalculateLoxodromicLine(
            new Coordinate(60, 0),
            new Coordinate(60, 10)
        );

        // Due east, the rhumb line is the parallel itself: R * cos(lat) * dLon.
        var expected = Radius * Math.Cos(60 * Math.PI / 180) * (10 * Math.PI / 180);

        Assert.Equal(expected, line.Distance.SiValue, expected * 1e-9);
        Assert.Equal(90, line.Bearing12, 1e-9);
        Assert.Equal(270, line.Bearing21, 1e-9);
    }

    [Fact]
    public void Loxodromic_line_along_a_meridian_follows_the_meridian()
    {
        var line = Calculator.CalculateLoxodromicLine(new Coordinate(0, 5), new Coordinate(10, 5));

        var expected = Radius * (10 * Math.PI / 180);

        Assert.Equal(expected, line.Distance.SiValue, expected * 1e-9);
        Assert.Equal(0, line.Bearing12, 1e-9);
        Assert.Equal(180, line.Bearing21, 1e-9);
    }

    [Fact]
    public void Loxodromic_course_is_the_slope_on_a_mercator_chart()
    {
        var line = Calculator.CalculateLoxodromicLine(new Coordinate(0, 0), new Coordinate(10, 10));

        // A rhumb line is straight on a Mercator chart, so its course is the ratio of the
        // longitude difference to the stretched latitude difference.
        var expected =
            Math.Atan2(10 * Math.PI / 180, Math.Log(Math.Tan(Math.PI / 4 + 5 * Math.PI / 180)))
            * 180
            / Math.PI;

        Assert.Equal(expected, line.Bearing12, 1e-9);
        // The bearing never changes along a rhumb line, so the return leg is its reciprocal.
        Assert.Equal(0, BearingDelta(line.Bearing21, line.Bearing12 + 180), 1e-9);
        // ... and it is longer than the great circle between the same two points.
        Assert.True(
            line.Distance
                > Calculator
                    .CalculateOrthodromicLine(new Coordinate(0, 0), new Coordinate(10, 10))
                    .Distance
        );
    }

    [Fact]
    public void Loxodromic_line_between_coincident_points_is_null()
    {
        Assert.Null(Calculator.CalculateLoxodromicLine(new Coordinate(1, 2), new Coordinate(1, 2)));
    }

    #endregion

    #region Meridional measures and sequence length

    [Theory]
    [InlineData(25)]
    [InlineData(-25)]
    [InlineData(0)]
    public void Meridional_distance_is_the_arc_from_the_equator(double latitude)
    {
        // A sphere's meridians are circles of a single radius, so the arc is R * latitude.
        var expected = Radius * latitude * Math.PI / 180;

        Assert.Equal(expected, Calculator.CalculateMeridionalDistance(latitude), 1e-6);
    }

    [Fact]
    public void Meridional_parts_are_the_mercator_ordinate_in_nautical_miles()
    {
        var expected = Radius * Math.Log(Math.Tan(Math.PI / 4 + 25 * Math.PI / 360)) / 1852;

        Assert.Equal(expected, Calculator.CalculateMeridionalParts(25), 1e-6);
        // Mercator is symmetric about the equator.
        Assert.Equal(-expected, Calculator.CalculateMeridionalParts(-25), 1e-6);
        Assert.Equal(0, Calculator.CalculateMeridionalParts(0), 1e-12);
    }

    [Fact]
    public void Length_of_a_sequence_is_the_sum_of_its_legs()
    {
        var first = new Coordinate(0, 0);
        var second = new Coordinate(0, 10);
        var third = new Coordinate(10, 10);

        var expected =
            Calculator.CalculateOrthodromicLine(first, second).Distance.SiValue
            + Calculator.CalculateOrthodromicLine(second, third).Distance.SiValue;

        var length = Calculator.CalculateLength(new CoordinateSequence(first, second, third));

        Assert.Equal(expected, length.SiValue, expected * 1e-9);
    }

    [Fact]
    public void Length_of_a_sequence_with_nothing_to_measure_is_zero()
    {
        Assert.Equal(0, Calculator.CalculateLength(new CoordinateSequence()).SiValue);
        Assert.Equal(
            0,
            Calculator.CalculateLength(new CoordinateSequence(new Coordinate(1, 2))).SiValue
        );
        // Repeated coordinates contribute nothing rather than faulting.
        Assert.Equal(
            0,
            Calculator
                .CalculateLength(new CoordinateSequence(new Coordinate(1, 2), new Coordinate(1, 2)))
                .SiValue
        );
    }

    #endregion

    [Theory]
    [InlineData(0, 0, 10, 10)]
    [InlineData(51.5, -0.1, 48.85, 2.35)]
    [InlineData(-33.9, 151.2, 35.7, 139.7)]
    public void Sphere_and_spheroid_agree_to_within_a_percent(
        double lat1,
        double lon1,
        double lat2,
        double lon2
    )
    {
        // The sphere is an approximation of the spheroid, not a different notion of
        // distance: the two must land in the same place.
        var a = new Coordinate(lat1, lon1);
        var b = new Coordinate(lat2, lon2);
        var spheroid = new SpheroidCalculator(Spheroid.Wgs84);

        var sphereGreatCircle = Calculator.CalculateOrthodromicLine(a, b);
        var spheroidGreatCircle = spheroid.CalculateOrthodromicLine(a, b);
        var sphereRhumb = Calculator.CalculateLoxodromicLine(a, b);
        var spheroidRhumb = spheroid.CalculateLoxodromicLine(a, b);

        Assert.Equal(
            spheroidGreatCircle.Distance.SiValue,
            sphereGreatCircle.Distance.SiValue,
            spheroidGreatCircle.Distance.SiValue * 0.01
        );
        Assert.Equal(
            spheroidRhumb.Distance.SiValue,
            sphereRhumb.Distance.SiValue,
            spheroidRhumb.Distance.SiValue * 0.01
        );
        Assert.Equal(
            0,
            BearingDelta(sphereGreatCircle.Bearing12, spheroidGreatCircle.Bearing12),
            0.5
        );
        Assert.Equal(
            0,
            BearingDelta(sphereGreatCircle.Bearing21, spheroidGreatCircle.Bearing21),
            0.5
        );
        Assert.Equal(0, BearingDelta(sphereRhumb.Bearing12, spheroidRhumb.Bearing12), 0.5);
    }
}
