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
    public void An_60Degree_CircleWith_111000M_RadiusShouldBeAboutOneDegreeWide()
    {
        var circle = new Circle(60, 20, 111000);
        var bounds = circle.GetBounds();

        var minLonError = Distance(19.5, bounds.MinLon);
        Assert.True(minLonError <= 0.002);

        var maxLonError = Distance(20.5, bounds.MaxLon);
        Assert.True(maxLonError <= 0.002);
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

    public double Distance(double nr1, double nr2)
    {
        return Math.Abs(nr1 - nr2);
    }
}
