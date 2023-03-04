using System.Collections.Generic;
using System.IO;
using System.Text;
using Geo.Geometries;

namespace Geo.IO.Google;

public class GooglePolylineEncoder
{
    // Attributions:
    // https://developers.google.com/maps/documentation/utilities/polylinealgorithm
    // http://geographyservices.codeplex.com
    // http://jeffreysambells.com/2010/05/27/decoding-polylines-from-google-maps-direction-api-with-java

    private const double CoordinateFactor = 1e5;
    private const int BinaryChunkSize = 5;
    private const int MinAscii = 63;

    public string Encode(LineString lineString)
    {
        var plat = 0;
        var plng = 0;

        var builder = new StringBuilder();

        foreach (var coordinate in lineString.Coordinates)
        {
            var late5 = (int)(coordinate.Latitude * CoordinateFactor);
            var lnge5 = (int)(coordinate.Longitude * CoordinateFactor);

            EncodeNumber(builder, late5 - plat);
            EncodeNumber(builder, lnge5 - plng);

            plat = late5;
            plng = lnge5;
        }

        return builder.ToString();
    }

    public LineString Decode(string polyline)
    {
        var coordinates = new List<Coordinate>();
        using (var reader = new StringReader(polyline))
        {
            int lat = 0, lng = 0;
            while (reader.Peek() != -1)
            {
                lat += DecodeNumber(reader);
                lng += DecodeNumber(reader);

                var p = new Coordinate(lat / CoordinateFactor, lng / CoordinateFactor);
                coordinates.Add(p);
            }
        }

        return new LineString(coordinates);
    }

    private static void EncodeNumber(StringBuilder builder, int num)
    {
        num = num << 1;

        if (num < 0)
            num = ~num;

        while (num >= 0x20)
        {
            builder.Append((char)((0x20 | (num & 0x1f)) + MinAscii));
            num >>= BinaryChunkSize;
        }

        builder.Append((char)(num + MinAscii));
    }

    private static int DecodeNumber(StringReader reader)
    {
        int b, shift = 0, result = 0;
        do
        {
            b = reader.Read() - MinAscii;
            result |= (b & 0x1f) << shift;
            shift += BinaryChunkSize;
        } while (b >= 0x20);

        return (result & 1) != 0 ? ~(result >> 1) : result >> 1;
    }
}