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
}
