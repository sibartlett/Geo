using System;
using Geo.Geodesy;
using Geo.Geometries;
using Geo.Measure;
using Xunit;

namespace Geo.Tests.Geodesy;

public class SpheroidCalculatorTests
{
    private const double Millionth = 0.000001;

    [Theory]
    [InlineData(25, 1543.030567)]
    [InlineData(-25, -1543.030567)]
    public void MeridionalParts(double latitude, double parts)
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateMeridionalParts(latitude);
        Assert.Equal(parts, result, Millionth);
    }

    [Theory]
    [InlineData(25, 1493.549767)]
    public void MeridionalDistance(double latitude, double parts)
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateMeridionalDistance(latitude);
        Assert.Equal(parts, result.ConvertTo(DistanceUnit.Nm), Millionth);
    }

    [Theory]
    [InlineData(0, 0, 10, 10, 845.100058)]
    public void CalculateLoxodromicLineDistance(
        double lat1,
        double lon1,
        double lat2,
        double lon2,
        double distance
    )
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateLoxodromicLine(
            new Point(lat1, lon1),
            new Point(lat2, lon2)
        );
        Assert.Equal(distance, result.Distance.ConvertTo(DistanceUnit.Nm).Value, Millionth);
    }

    [Theory]
    [InlineData(0, 0, 10, 10, 45.044293)]
    public void CalculateLoxodromicCourse(
        double lat1,
        double lon1,
        double lat2,
        double lon2,
        double distance
    )
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateLoxodromicLine(
            new Point(lat1, lon1),
            new Point(lat2, lon2)
        );
        Assert.Equal(distance, result.Bearing12, Millionth);
    }

    [Theory]
    [InlineData(0, 0, 10, 10, 44.751910, 225.629037)]
    public void CalculateOrthodromicCourse(
        double lat1,
        double lon1,
        double lat2,
        double lon2,
        double c12,
        double c21
    )
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateOrthodromicLine(
            new Point(lat1, lon1),
            new Point(lat2, lon2)
        );
        Assert.Equal(c12, result.Bearing12, Millionth);
        Assert.Equal(c21, result.Bearing21, Millionth);
    }

    [Theory]
    [InlineData(0, 0, 56, 34, 0.318436, 0.468951)]
    [InlineData(-9.443333, 147.216667, 327.912522, 50, -8.733717, 146.769644)]
    public void CalculateOrthodromicDestination(
        double lat1,
        double lon1,
        double angle,
        double distance,
        double lat2,
        double lon2
    )
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateOrthodromicLine(
            new Point(lat1, lon1),
            angle,
            new Distance(distance, DistanceUnit.Nm).SiValue
        );
        Assert.Equal(lat2, result.Coordinate2.Latitude, Millionth);
        Assert.Equal(lon2, result.Coordinate2.Longitude, Millionth);
    }

    // Regression test for https://github.com/sibartlett/Geo/issues/7:
    // near-antipodal points (lat1 == -lat2) used to make Vincenty's inverse
    // routine fail to converge and throw an ArithmeticException.
    [Theory]
    [InlineData(30, 175, -30, -3.5, 10736.730329, 269.755739, 89.755739)]
    [InlineData(30, 176, -30, -3.5, 10761.582116, 269.874999, 89.874999)]
    public void CalculateOrthodromicLine_converges_for_near_antipodal_points(
        double lat1,
        double lon1,
        double lat2,
        double lon2,
        double distance,
        double bearing12,
        double bearing21
    )
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var result = calculator.CalculateOrthodromicLine(
            new Point(lat1, lon1),
            new Point(lat2, lon2)
        );
        Assert.Equal(distance, result.Distance.ConvertTo(DistanceUnit.Nm).Value, Millionth);
        Assert.Equal(bearing12, result.Bearing12, Millionth);
        Assert.Equal(bearing21, result.Bearing21, Millionth);
    }

    #region Areas and lengths answer for the spheroid they were given

    // These used to be handed to a hardcoded sphere of the WGS-84 mean radius, so the
    // Spheroid passed to the constructor made no difference to any of them: a planet half
    // the size returned bit-identical areas.

    [Fact]
    public void Envelope_area_of_the_whole_world_is_the_spheroids_surface_area()
    {
        // 2*pi*a^2 + pi*(b^2/e)*ln((1+e)/(1-e)), the closed form for an oblate spheroid.
        var spheroid = Spheroid.Wgs84;
        var e = spheroid.Eccentricity;
        var b = spheroid.PolarAxis;
        var expected =
            2 * Math.PI * spheroid.EquatorialAxis * spheroid.EquatorialAxis
            + Math.PI * (b * b / e) * Math.Log((1 + e) / (1 - e));

        var area = new SpheroidCalculator(spheroid)
            .CalculateArea(new Envelope(-90, -180, 90, 180))
            .SiValue;

        Assert.Equal(expected, area, expected * 1e-12);
    }

    [Fact]
    public void Envelope_perimeter_of_the_whole_world_is_the_meridian_circumference()
    {
        // North pole to south pole and back: the sides are the whole meridian, and the
        // parallels at the poles have no length at all.
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);

        var expected = 4 * calculator.CalculateMeridionalDistance(90);
        var perimeter = calculator.CalculateLength(new Envelope(-90, -180, 90, 180)).SiValue;

        Assert.Equal(expected, perimeter, expected * 1e-12);
    }

    [Theory]
    [InlineData(0, 10)]
    [InlineData(40, 50)]
    [InlineData(-60, -50)]
    [InlineData(80, 89)]
    public void Ring_area_around_a_box_matches_the_envelopes_own_area(double minLat, double maxLat)
    {
        // Two independent routes to the same number - the zone formula and the ring
        // formula with authalic latitudes - which must agree.
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);

        var envelope = calculator.CalculateArea(new Envelope(minLat, 0, maxLat, 10)).SiValue;
        var ring = calculator
            .CalculateArea(
                new CoordinateSequence(
                    new Coordinate(minLat, 0),
                    new Coordinate(minLat, 10),
                    new Coordinate(maxLat, 10),
                    new Coordinate(maxLat, 0),
                    new Coordinate(minLat, 0)
                )
            )
            .SiValue;

        Assert.Equal(envelope, ring, envelope * 1e-12);
    }

    [Fact]
    public void A_smaller_planet_has_proportionally_smaller_areas_and_lengths()
    {
        var full = new SpheroidCalculator(new Spheroid("full", 6378137, 298.257223563));
        var half = new SpheroidCalculator(new Spheroid("half", 6378137d / 2, 298.257223563));
        var envelope = new Envelope(0, 0, 10, 10);
        var ring = new CoordinateSequence(
            new Coordinate(0, 0),
            new Coordinate(0, 10),
            new Coordinate(10, 10),
            new Coordinate(10, 0),
            new Coordinate(0, 0)
        );

        // Area goes with the square of the radius, length with the radius itself.
        Assert.Equal(
            full.CalculateArea(envelope).SiValue / 4,
            half.CalculateArea(envelope).SiValue,
            full.CalculateArea(envelope).SiValue * 1e-12
        );
        Assert.Equal(
            full.CalculateArea(ring).SiValue / 4,
            half.CalculateArea(ring).SiValue,
            full.CalculateArea(ring).SiValue * 1e-12
        );
        Assert.Equal(
            full.CalculateLength(envelope).SiValue / 2,
            half.CalculateLength(envelope).SiValue,
            full.CalculateLength(envelope).SiValue * 1e-12
        );
    }

    [Fact]
    public void Different_datums_give_different_answers()
    {
        var envelope = new Envelope(0, 0, 10, 10);
        var wgs84 = new SpheroidCalculator(Spheroid.Wgs84);
        var clarke = new SpheroidCalculator(Spheroid.Clarke1866);
        var international = new SpheroidCalculator(Spheroid.International1924);

        Assert.NotEqual(
            wgs84.CalculateArea(envelope).SiValue,
            clarke.CalculateArea(envelope).SiValue
        );
        Assert.NotEqual(
            wgs84.CalculateArea(envelope).SiValue,
            international.CalculateArea(envelope).SiValue
        );
        Assert.NotEqual(
            wgs84.CalculateLength(envelope).SiValue,
            clarke.CalculateLength(envelope).SiValue
        );

        // ... but they are all the same planet, so only just.
        Assert.Equal(
            wgs84.CalculateArea(envelope).SiValue,
            clarke.CalculateArea(envelope).SiValue,
            wgs84.CalculateArea(envelope).SiValue * 0.001
        );
    }

    [Fact]
    public void An_envelope_with_no_extent_encloses_nothing()
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);

        foreach (var latitude in new[] { -90d, -45, 0, 45, 90 })
        {
            Assert.Equal(
                0,
                calculator.CalculateArea(new Envelope(latitude, 5, latitude, 5)).SiValue
            );
            Assert.Equal(
                0,
                calculator.CalculateArea(new Envelope(latitude, 0, latitude, 10)).SiValue
            );
        }
    }

    #endregion

    [Fact]
    public void Direct_solution_reports_the_back_azimuth_in_degrees()
    {
        // Setting off due east from the equator, the way back is due west. The back
        // azimuth used to be left in radians, so this returned 4.712389 - a number small
        // enough to pass for a bearing, which is exactly why it went unnoticed.
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);

        var result = calculator.CalculateOrthodromicLine(new Point(0, 0), 90, 100000);

        Assert.Equal(270, result.Bearing21, Millionth);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(45)]
    [InlineData(90)]
    [InlineData(135)]
    [InlineData(270)]
    public void Direct_and_inverse_solutions_agree_on_both_bearings(double heading)
    {
        var calculator = new SpheroidCalculator(Spheroid.Wgs84);
        var start = new Point(51.5, -0.1);

        var direct = calculator.CalculateOrthodromicLine(start, heading, 100000);
        var inverse = calculator.CalculateOrthodromicLine(start, direct.Coordinate2);

        Assert.Equal(direct.Bearing12, inverse.Bearing12, Millionth);
        Assert.Equal(direct.Bearing21, inverse.Bearing21, Millionth);
    }
}
