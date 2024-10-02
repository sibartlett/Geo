using Geo.Geometries;
using Geo.IO.Google;
using Xunit;

namespace Geo.Tests.IO.Google;

public class GooglePolylineEncoderTests
{
    [Fact]
    public void Encode()
    {
        var lineString = new LineString(
            new Coordinate(38.5, -120.2),
            new Coordinate(40.7, -120.95),
            new Coordinate(43.252, -126.453)
        );

        var result = new GooglePolylineEncoder().Encode(lineString);

        Assert.Equal("_p~iF~ps|U_ulLnnqC_mqNvxq`@", result);
    }

    [Fact]
    public void Decode()
    {
        var lineString = new LineString(
            new Coordinate(38.5, -120.2),
            new Coordinate(40.7, -120.95),
            new Coordinate(43.252, -126.453)
        );

        var result = new GooglePolylineEncoder().Decode("_p~iF~ps|U_ulLnnqC_mqNvxq`@");

        Assert.Equal(lineString, result);
    }
}
