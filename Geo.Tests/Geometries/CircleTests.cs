using System;
using Geo.Geometries;
using Xunit;

namespace Geo.Tests.Geometries;

public class CircleTests
{
    [Fact]
    public void AnEquatorialCircleWith_111000M_RadiusShouldBeAboutTwoDegreesTall()
    {
        var circle = new Circle(0, 20, 111000);
        var bounds = circle.GetBounds();

        var minLatError = Distance(-1, bounds.MinLat);
        Assert.True(minLatError <= 0.002);

        var maxLatError = Distance(+1, bounds.MaxLat);
        Assert.True(maxLatError <= 0.002);
    }

    [Fact]
    public void Bounds_A_111000_RadiusMeterEquatorialCircleShouldBeAboutTwoDegreesWide()
    {
        var circle = new Circle(0, 20, 111000);
        var bounds = circle.GetBounds();

        var minLonError = Distance(19, bounds.MinLon);
        Assert.True(minLonError <= 0.002);

        var maxLonError = Distance(21, bounds.MaxLon);
        Assert.True(maxLonError <= 0.002);
    }

    [Fact]
    public void An_60Degree_CircleWith_111000M_RadiusShouldBeAboutTwoDegreesTall()
    {
        var circle = new Circle(60, 20, 111000);
        var bounds = circle.GetBounds();

        var minLatError = Distance(59, bounds.MinLat);
        Assert.True(minLatError <= 0.002);

        var maxLatError = Distance(61, bounds.MaxLat);
        Assert.True(maxLatError <= 0.002);
    }

    [Fact]
    public void An_60Degree_CircleWith_111000M_RadiusShouldBeAboutFourDegreesWide()
    {
        var circle = new Circle(60, 20, 111000);
        var bounds = circle.GetBounds();

        // At 60N a degree of longitude spans only cos(60) = half the metres of a degree
        // of latitude, so a circle that is ~2 degrees tall is ~4 degrees wide (about two
        // degrees either side of the centre).
        var minLonError = Distance(18.002, bounds.MinLon);
        Assert.True(minLonError <= 0.002);

        var maxLonError = Distance(21.998, bounds.MaxLon);
        Assert.True(maxLonError <= 0.002);
    }

    [Theory]
    [InlineData(89.5, 100000)] // reaches over the north pole
    [InlineData(-89.5, 100000)] // reaches over the south pole
    [InlineData(89.9, 100000)] // longitudinal half-span would be several turns
    [InlineData(0, 40000000)] // larger than the earth
    public void Bounds_of_a_circle_that_reaches_a_pole_stay_inside_the_valid_range(
        double latitude,
        double radius
    )
    {
        var bounds = new Circle(latitude, 0, radius).GetBounds();

        Assert.InRange(bounds.MinLat, -90, 90);
        Assert.InRange(bounds.MaxLat, -90, 90);
        Assert.InRange(bounds.MinLon, -180, 180);
        Assert.InRange(bounds.MaxLon, -180, 180);

        // Every meridian runs through a circle that covers a pole, so its box has to
        // span the whole longitude range rather than a slice of it.
        Assert.Equal(-180, bounds.MinLon);
        Assert.Equal(180, bounds.MaxLon);
    }

    [Fact]
    public void Bounds_of_a_circle_crossing_the_anti_meridian_span_every_longitude()
    {
        // An envelope runs west to east, so it cannot describe a wrapped box; the whole
        // range is the smallest one it can express that still contains the circle.
        var bounds = new Circle(0, 179.9, 100000).GetBounds();

        Assert.Equal(-180, bounds.MinLon);
        Assert.Equal(180, bounds.MaxLon);
    }

    [Fact]
    public void Bounds_widen_towards_the_pole_without_ever_leaving_the_valid_range()
    {
        var previousWidth = 0d;

        foreach (var latitude in new[] { 0d, 30d, 45d, 60d, 75d, 80d, 85d, 88d, 89d })
        {
            var bounds = new Circle(latitude, 20, 100000).GetBounds();

            Assert.InRange(bounds.MinLat, -90, 90);
            Assert.InRange(bounds.MaxLat, -90, 90);
            Assert.InRange(bounds.MinLon, -180, 180);
            Assert.InRange(bounds.MaxLon, -180, 180);

            // The parallels converge towards the pole, so the same metric radius covers
            // more of them the further north the circle sits.
            var width = bounds.MaxLon - bounds.MinLon;
            Assert.True(width > previousWidth, "width did not grow at " + latitude);
            previousWidth = width;
        }
    }

    [Fact]
    public void Longitudinal_bounds_reach_the_meridians_tangent_to_the_circle()
    {
        // The widest meridians a circle touches are at asin(sin r / cos lat) from its
        // centre, not the r / cos(lat) small-angle approximation, which understates them.
        const double latitude = 80;
        var radiusDeg = 100000 / (1852d * 60);
        var bounds = new Circle(latitude, 20, 100000).GetBounds();

        var expected =
            Math.Asin(Math.Sin(radiusDeg * Math.PI / 180) / Math.Cos(latitude * Math.PI / 180))
            * 180
            / Math.PI;

        Assert.Equal(20 - expected, bounds.MinLon, 9);
        Assert.Equal(20 + expected, bounds.MaxLon, 9);
        Assert.True(expected > radiusDeg / Math.Cos(latitude * Math.PI / 180));
    }

    [Fact]
    public void Empty_circle_has_no_bounds()
    {
        Assert.True(new Circle().IsEmpty);
        Assert.Null(new Circle().GetBounds());
        Assert.Null(Circle.Empty.GetBounds());
    }

    [Fact]
    public void GetArea_approximates_pi_r_squared()
    {
        const double radius = 111000;
        var circle = new Circle(0, 20, radius);

        var expected = Math.PI * radius * radius;

        Assert.Equal(expected, circle.GetArea().SiValue, expected * 0.01);
    }

    [Fact]
    public void GetLength_approximates_the_circumference()
    {
        const double radius = 111000;
        var circle = new Circle(0, 20, radius);

        var expected = 2 * Math.PI * radius;

        Assert.Equal(expected, circle.GetLength().SiValue, expected * 0.01);
    }

    [Fact]
    public void ToPolygon_produces_a_closed_ring_with_one_coordinate_per_side()
    {
        var polygon = new Circle(0, 20, 111000).ToPolygon(8);

        Assert.False(polygon.IsEmpty);
        Assert.True(polygon.Shell.IsClosed);
        // eight vertices plus the repeated closing coordinate
        Assert.Equal(9, polygon.Shell.Coordinates.Count);
    }

    [Fact]
    public void ToPolygon_requires_at_least_three_sides()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new Circle(0, 20, 111000).ToPolygon(2));
    }

    [Fact]
    public void Empty_circles_report_empty()
    {
        Assert.True(new Circle().IsEmpty);
        Assert.True(Circle.Empty.IsEmpty);
        Assert.False(new Circle(0, 20, 111000).IsEmpty);
    }

    [Fact]
    public void Equality_compares_center_and_radius()
    {
        Assert.True(new Circle(0, 20, 111000) == new Circle(0, 20, 111000));
        Assert.True(new Circle(0, 20, 111000) != new Circle(0, 20, 222000));
        Assert.True(new Circle(0, 20, 111000) != new Circle(1, 20, 111000));
    }

    [Fact]
    public void Equality_via_object_overload_and_hashcode()
    {
        var circle = new Circle(0, 20, 111000);
        var same = new Circle(0, 20, 111000);

        Assert.True(circle.Equals((object)same));
        Assert.False(circle.Equals((object?)null));
        Assert.False(circle.Equals((object)"not a circle"));
        Assert.Equal(circle.GetHashCode(), same.GetHashCode());
    }

    [Fact]
    public void Equality_operators_handle_null()
    {
        var circle = new Circle(0, 20, 111000);

        Assert.True((Circle)null == (Circle)null);
        Assert.False(circle == null);
        Assert.False(null == circle);
        Assert.True(circle != null);
    }

    [Fact]
    public void Center_constructed_with_elevation_is_3d_but_not_measured()
    {
        var circle = new Circle(1, 2, 300, 111000);

        Assert.True(circle.Is3D);
        Assert.False(circle.IsMeasured);
        Assert.Equal(300, ((CoordinateZ)circle.Center).Elevation);
        Assert.Equal(111000, circle.Radius);
    }

    [Fact]
    public void Center_constructed_with_elevation_and_measure_is_3d_and_measured()
    {
        var circle = new Circle(1, 2, 300, 42, 111000);

        Assert.True(circle.Is3D);
        Assert.True(circle.IsMeasured);
        var center = (CoordinateZM)circle.Center;
        Assert.Equal(300, center.Elevation);
        Assert.Equal(42, center.Measure);
    }

    [Fact]
    public void Center_from_coordinate_constructor_reports_dimensions_from_the_centre()
    {
        var circle = new Circle(new Coordinate(1, 2), 111000);

        Assert.False(circle.Is3D);
        Assert.False(circle.IsMeasured);
    }

    [Fact]
    public void Empty_circle_is_neither_3d_nor_measured()
    {
        Assert.False(new Circle().Is3D);
        Assert.False(new Circle().IsMeasured);
    }

    [Fact]
    public void Empty_circle_converts_to_the_empty_polygon()
    {
        // An empty circle has no centre to project vertices from; the writers that
        // convert circles to polygons rely on this rather than faulting.
        Assert.True(Circle.Empty.ToPolygon().IsEmpty);
    }

    public double Distance(double nr1, double nr2)
    {
        return Math.Abs(nr1 - nr2);
    }
}
