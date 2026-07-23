using System;
using System.Diagnostics.CodeAnalysis;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;

namespace Geo.IO;

/// <summary>
/// Detects which textual geospatial format a string is in and parses it, without the
/// caller having to know the format up front. Supported formats are coordinate strings
/// (<see cref="Coordinate" />), Well-Known Text (<see cref="WktReader" />) and GeoJSON
/// (<see cref="GeoJsonReader" />).
/// </summary>
public static class GeoFormat
{
    /// <summary>
    /// Detects the format of <paramref name="input" /> without returning the parsed value.
    /// This performs a full parse attempt, so <see cref="Detect" /> and <see cref="TryParse" />
    /// always agree on the format.
    /// </summary>
    /// <param name="input">The string to inspect.</param>
    /// <returns>
    /// The detected <see cref="GeoStringFormat" />, or <see cref="GeoStringFormat.Unknown" />
    /// if the string matched no supported format.
    /// </returns>
    public static GeoStringFormat Detect(string input)
    {
        TryParse(input, out _, out var format);
        return format;
    }

    /// <summary>
    /// Detects the format of <paramref name="input" /> and parses it.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <returns>
    /// The parsed value: a <see cref="Coordinate" />, an
    /// <see cref="Geo.Abstractions.Interfaces.IGeometry" /> (WKT) or an
    /// <see cref="Geo.Abstractions.Interfaces.IGeoJsonObject" /> (GeoJSON).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="input" /> is <c>null</c>.</exception>
    /// <exception cref="FormatException"><paramref name="input" /> matched no supported format.</exception>
    public static object Parse(string input)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input));

        if (!TryParse(input, out var result, out _))
            throw new FormatException("Value (" + input + ") is not a supported geo format.");

        return result;
    }

    /// <summary>
    /// Attempts to detect the format of <paramref name="input" /> and parse it.
    /// </summary>
    /// <param name="input">The string to parse.</param>
    /// <param name="result">
    /// When this method returns <c>true</c>, the parsed value (a <see cref="Coordinate" />, WKT
    /// <see cref="Geo.Abstractions.Interfaces.IGeometry" /> or GeoJSON
    /// <see cref="Geo.Abstractions.Interfaces.IGeoJsonObject" />); otherwise <c>null</c>.
    /// </param>
    /// <param name="format">
    /// When this method returns <c>true</c>, the detected format; otherwise
    /// <see cref="GeoStringFormat.Unknown" />.
    /// </param>
    /// <returns><c>true</c> if the string was recognised and parsed; otherwise <c>false</c>.</returns>
    public static bool TryParse(
        string input,
        [NotNullWhen(true)] out object? result,
        out GeoStringFormat format
    )
    {
        result = null;
        format = GeoStringFormat.Unknown;

        if (string.IsNullOrWhiteSpace(input))
            return false;

        // Pick a candidate parser from the first meaningful character, then delegate to the
        // existing reader. A leading brace/bracket can be either GeoJSON or a braced coordinate
        // string (the coordinate regex allows { [ ( wrappers), so GeoJSON is tried first and
        // falls back to a coordinate parse. A leading ASCII letter means WKT. Anything else is
        // treated as a coordinate string.
        var first = FirstNonWhitespace(input);

        if (first == '{' || first == '[')
        {
            if (TryGeoJson(input, out result))
            {
                format = GeoStringFormat.GeoJson;
                return true;
            }

            if (TryCoordinate(input, out result))
            {
                format = GeoStringFormat.Coordinate;
                return true;
            }

            return false;
        }

        if ((first >= 'A' && first <= 'Z') || (first >= 'a' && first <= 'z'))
        {
            if (TryWkt(input, out result))
            {
                format = GeoStringFormat.Wkt;
                return true;
            }

            return false;
        }

        if (TryCoordinate(input, out result))
        {
            format = GeoStringFormat.Coordinate;
            return true;
        }

        return false;
    }

    private static char FirstNonWhitespace(string input)
    {
        foreach (var c in input)
            if (!char.IsWhiteSpace(c))
                return c;
        return '\0';
    }

    private static bool TryCoordinate(string input, [NotNullWhen(true)] out object? result)
    {
        if (Coordinate.TryParse(input, out var coordinate))
        {
            result = coordinate;
            return true;
        }

        result = null;
        return false;
    }

    private static bool TryWkt(string input, [NotNullWhen(true)] out object? result)
    {
        try
        {
            var geometry = new WktReader().Read(input);
            if (geometry != null)
            {
                result = geometry;
                return true;
            }
        }
        catch (Exception)
        {
            // Not valid WKT; fall through.
        }

        result = null;
        return false;
    }

    private static bool TryGeoJson(string input, [NotNullWhen(true)] out object? result)
    {
        try
        {
            if (new GeoJsonReader().TryRead(input, out var obj) && obj != null)
            {
                result = obj;
                return true;
            }
        }
        catch (Exception)
        {
            // Not valid GeoJSON; fall through.
        }

        result = null;
        return false;
    }
}
