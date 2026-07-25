#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
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
            // The Google polyline algorithm rounds the scaled ordinate to the nearest
            // integer; truncating with a plain (int) cast loses the last digit for
            // common coordinates (e.g. 1.234567) and produces output that disagrees
            // with Google's reference encoder by one unit in the last place.
            var late5 = (int)
                Math.Round(coordinate.Latitude * CoordinateFactor, MidpointRounding.AwayFromZero);
            var lnge5 = (int)
                Math.Round(coordinate.Longitude * CoordinateFactor, MidpointRounding.AwayFromZero);

            EncodeNumber(builder, late5 - plat);
            EncodeNumber(builder, lnge5 - plng);

            plat = late5;
            plng = lnge5;
        }

        return builder.ToString();
    }

    public LineString Decode(string polyline)
    {
        if (polyline == null)
            throw new ArgumentNullException("polyline");

        var coordinates = new List<Coordinate>();
        var index = 0;
        int lat = 0,
            lng = 0;

        while (index < polyline.Length)
        {
            lat += DecodeNumber(polyline, ref index);
            lng += DecodeNumber(polyline, ref index);

            coordinates.Add(CreateCoordinate(lat, lng));
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

    // Each number is a run of 5-bit chunks, low chunk first, carried in the low six
    // bits of a printable ASCII character offset by 63; the sixth bit is set on every
    // chunk but the last. The end of the string, a character outside that range, and a
    // legitimate final chunk must be told apart: masking the chunk before checking
    // would turn both malformed cases into data bits and decode them silently.
    private static int DecodeNumber(string polyline, ref int index)
    {
        var start = index;
        int b,
            shift = 0,
            result = 0;

        do
        {
            if (index >= polyline.Length)
                throw new SerializationException(
                    "Polyline ended part-way through the number starting at offset "
                        + start.ToString(CultureInfo.InvariantCulture)
                        + "."
                );

            var ch = polyline[index++];
            b = ch - MinAscii;

            if (b < 0 || b > 0x3f)
                throw new SerializationException(
                    "Invalid polyline character '"
                        + ch
                        + "' at offset "
                        + (index - 1).ToString(CultureInfo.InvariantCulture)
                        + "."
                );

            // The chunks of a single number cannot carry more bits than the int they
            // accumulate into; without this the shift wraps (C# masks it to 0-31) and
            // a runaway sequence of chunks silently folds back over the low bits.
            if (shift >= 32)
                throw new SerializationException(
                    "The number starting at offset "
                        + start.ToString(CultureInfo.InvariantCulture)
                        + " is too large for a polyline ordinate."
                );

            result |= (b & 0x1f) << shift;
            shift += BinaryChunkSize;
        } while (b >= 0x20);

        return (result & 1) != 0 ? ~(result >> 1) : result >> 1;
    }

    // A polyline that decodes to an impossible position is a malformed document, not a
    // bad argument from the caller, so it is reported the same way as every other
    // decoding failure instead of as the ArgumentOutOfRangeException the constructor
    // would raise (which names 'latitude', a parameter the caller never supplied).
    private static Coordinate CreateCoordinate(int lat, int lng)
    {
        try
        {
            return new Coordinate(lat / CoordinateFactor, lng / CoordinateFactor);
        }
        catch (ArgumentOutOfRangeException ex)
        {
            throw new SerializationException(
                "Polyline decoded to a coordinate outside the valid range.",
                ex
            );
        }
    }
}
