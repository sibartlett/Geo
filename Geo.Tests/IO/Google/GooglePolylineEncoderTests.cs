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
    public void Encode_rounds_the_scaled_ordinate_to_the_nearest_unit()
    {
        // Regression: the scaled ordinate must be rounded (as the Google polyline
        // algorithm specifies), not truncated toward zero. 1.234567 * 1e5 = 123456.7,
        // which rounds to 123457; truncation would give 123456 and produce "_cpF_cpF".
        var lineString = new LineString(new Coordinate(1.234567, 1.234567));

        var result = new GooglePolylineEncoder().Encode(lineString);

        Assert.Equal("acpFacpF", result);
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
