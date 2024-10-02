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

    public double Distance(double nr1, double nr2)
    {
        return Math.Abs(nr1 - nr2);
    }
}
