#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;

namespace Geo.IO.GeoJson;

/// <summary>
/// A parsed JSON object: an ordered map of string keys to JSON values
/// (<see cref="JsonObject"/>, <see cref="JsonArray"/>, <see cref="string"/>,
/// <see cref="long"/>, <see cref="double"/>, <see cref="bool"/>, or <c>null</c>).
/// </summary>
internal sealed class JsonObject : Dictionary<string, object> { }

/// <summary>
/// A parsed JSON array of values.
/// </summary>
internal sealed class JsonArray : List<object> { }

/// <summary>
/// Minimal JSON reader/writer used by the GeoJSON serializers. It covers only
/// what GeoJSON needs: parsing a JSON string into a DOM of
/// <see cref="JsonObject"/>/<see cref="JsonArray"/>/primitive values, and
/// serializing dictionary/array trees back to a string. It replaces the
/// previously vendored SimpleJson implementation.
/// </summary>
internal static class Json
{
    /// <summary>
    /// Parses a JSON string. Returns <c>true</c> on success, with
    /// <paramref name="result"/> holding a <see cref="JsonObject"/>,
    /// <see cref="JsonArray"/>, <see cref="string"/>, <see cref="long"/>,
    /// <see cref="double"/>, <see cref="bool"/> or <c>null</c>.
    /// </summary>
    public static bool TryParse(string? json, out object? result)
    {
        if (json == null)
        {
            // Preserve the previous behaviour: a null input parses to a null
            // value without being treated as a failure.
            result = null;
            return true;
        }

        var success = true;
        result = new Parser(json).ParseValue(ref success);
        return success;
    }

    /// <summary>
    /// Serializes a dictionary/array/primitive tree to a JSON string.
    /// </summary>
    public static string Serialize(object? value)
    {
        var builder = new StringBuilder();
        SerializeValue(value, builder);
        return builder.ToString();
    }

    private sealed class Parser
    {
        private const string NumberChars = "0123456789+-.eE";
        private readonly string _s;
        private int _i;

        public Parser(string s) => _s = s;

        public object? ParseValue(ref bool success)
        {
            SkipWhitespace();
            if (_i >= _s.Length)
            {
                success = false;
                return null;
            }

            var c = _s[_i];
            switch (c)
            {
                case '{':
                    return ParseObject(ref success);
                case '[':
                    return ParseArray(ref success);
                case '"':
                    return ParseString(ref success);
                case 't':
                case 'f':
                    return ParseBool(ref success);
                case 'n':
                    return ParseNull(ref success);
                default:
                    if (c == '-' || (c >= '0' && c <= '9'))
                        return ParseNumber(ref success);
                    success = false;
                    return null;
            }
        }

        private JsonObject? ParseObject(ref bool success)
        {
            var obj = new JsonObject();
            _i++; // consume '{'

            while (true)
            {
                SkipWhitespace();
                if (_i >= _s.Length)
                {
                    success = false;
                    return null;
                }

                var c = _s[_i];
                if (c == '}')
                {
                    _i++;
                    return obj;
                }

                if (c == ',')
                {
                    _i++;
                    continue;
                }

                if (c != '"')
                {
                    success = false;
                    return null;
                }

                var name = ParseString(ref success);
                if (!success)
                    return null;

                SkipWhitespace();
                if (_i >= _s.Length || _s[_i] != ':')
                {
                    success = false;
                    return null;
                }
                _i++; // consume ':'

                var value = ParseValue(ref success);
                if (!success)
                    return null;

                // The DOM stores values as non-nullable object to keep the read
                // path free of nullable-cast warnings; JSON null is still held
                // as a null reference, matching the previous implementation.
                obj[name!] = value!;
            }
        }

        private JsonArray? ParseArray(ref bool success)
        {
            var array = new JsonArray();
            _i++; // consume '['

            while (true)
            {
                SkipWhitespace();
                if (_i >= _s.Length)
                {
                    success = false;
                    return null;
                }

                var c = _s[_i];
                if (c == ']')
                {
                    _i++;
                    return array;
                }

                if (c == ',')
                {
                    _i++;
                    continue;
                }

                var value = ParseValue(ref success);
                if (!success)
                    return null;

                array.Add(value!);
            }
        }

        private string? ParseString(ref bool success)
        {
            var sb = new StringBuilder();
            _i++; // consume opening '"'

            while (_i < _s.Length)
            {
                var c = _s[_i++];
                if (c == '"')
                    return sb.ToString();

                if (c != '\\')
                {
                    sb.Append(c);
                    continue;
                }

                if (_i >= _s.Length)
                    break;

                var e = _s[_i++];
                switch (e)
                {
                    case '"':
                        sb.Append('"');
                        break;
                    case '\\':
                        sb.Append('\\');
                        break;
                    case '/':
                        sb.Append('/');
                        break;
                    case 'b':
                        sb.Append('\b');
                        break;
                    case 'f':
                        sb.Append('\f');
                        break;
                    case 'n':
                        sb.Append('\n');
                        break;
                    case 'r':
                        sb.Append('\r');
                        break;
                    case 't':
                        sb.Append('\t');
                        break;
                    case 'u':
                        if (
                            _i + 4 > _s.Length
                            || !ushort.TryParse(
                                _s.Substring(_i, 4),
                                NumberStyles.HexNumber,
                                CultureInfo.InvariantCulture,
                                out var code
                            )
                        )
                        {
                            success = false;
                            return null;
                        }
                        // Each \uXXXX escape maps to one UTF-16 code unit; a
                        // surrogate pair arrives as two escapes and reassembles
                        // naturally as two appended chars.
                        sb.Append((char)code);
                        _i += 4;
                        break;
                    default:
                        success = false;
                        return null;
                }
            }

            success = false;
            return null;
        }

        private object? ParseNumber(ref bool success)
        {
            var start = _i;
            while (_i < _s.Length && NumberChars.IndexOf(_s[_i]) != -1)
                _i++;

            var str = _s.Substring(start, _i - start);

            if (str.IndexOf('.') != -1 || str.IndexOf('e') != -1 || str.IndexOf('E') != -1)
            {
                success = double.TryParse(
                    str,
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out var d
                );
                return d;
            }

            success = long.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out var l);
            return l;
        }

        private object? ParseBool(ref bool success)
        {
            if (Matches("true"))
            {
                _i += 4;
                return true;
            }
            if (Matches("false"))
            {
                _i += 5;
                return false;
            }
            success = false;
            return null;
        }

        private object? ParseNull(ref bool success)
        {
            if (Matches("null"))
            {
                _i += 4;
                return null;
            }
            success = false;
            return null;
        }

        private bool Matches(string literal)
        {
            if (_i + literal.Length > _s.Length)
                return false;
            return string.CompareOrdinal(_s, _i, literal, 0, literal.Length) == 0;
        }

        private void SkipWhitespace()
        {
            while (_i < _s.Length)
            {
                var c = _s[_i];
                if (c != ' ' && c != '\t' && c != '\n' && c != '\r')
                    break;
                _i++;
            }
        }
    }

    private static void SerializeValue(object? value, StringBuilder builder)
    {
        switch (value)
        {
            case null:
                builder.Append("null");
                break;
            case string s:
                SerializeString(s, builder);
                break;
            case bool b:
                builder.Append(b ? "true" : "false");
                break;
            case IDictionary<string, object?> dict:
                SerializeObject(dict, builder);
                break;
            case IEnumerable enumerable:
                SerializeArray(enumerable, builder);
                break;
            default:
                SerializeNumber(value, builder);
                break;
        }
    }

    private static void SerializeObject(
        IEnumerable<KeyValuePair<string, object?>> members,
        StringBuilder builder
    )
    {
        builder.Append('{');
        var first = true;
        foreach (var member in members)
        {
            if (!first)
                builder.Append(',');
            SerializeString(member.Key, builder);
            builder.Append(':');
            SerializeValue(member.Value, builder);
            first = false;
        }
        builder.Append('}');
    }

    private static void SerializeArray(IEnumerable array, StringBuilder builder)
    {
        builder.Append('[');
        var first = true;
        foreach (var value in array)
        {
            if (!first)
                builder.Append(',');
            SerializeValue(value, builder);
            first = false;
        }
        builder.Append(']');
    }

    private static void SerializeString(string value, StringBuilder builder)
    {
        builder.Append('"');
        foreach (var c in value)
        {
            switch (c)
            {
                case '"':
                    builder.Append("\\\"");
                    break;
                case '\\':
                    builder.Append("\\\\");
                    break;
                case '\b':
                    builder.Append("\\b");
                    break;
                case '\f':
                    builder.Append("\\f");
                    break;
                case '\n':
                    builder.Append("\\n");
                    break;
                case '\r':
                    builder.Append("\\r");
                    break;
                case '\t':
                    builder.Append("\\t");
                    break;
                default:
                    if (c < ' ')
                        builder.Append("\\u").Append(((int)c).ToString("x4"));
                    else
                        builder.Append(c);
                    break;
            }
        }
        builder.Append('"');
    }

    private static void SerializeNumber(object value, StringBuilder builder)
    {
        switch (value)
        {
            case double d:
                builder.Append(d.ToString("r", CultureInfo.InvariantCulture));
                break;
            case float f:
                builder.Append(f.ToString("r", CultureInfo.InvariantCulture));
                break;
            case decimal m:
                builder.Append(m.ToString(CultureInfo.InvariantCulture));
                break;
            case sbyte
            or byte
            or short
            or ushort
            or int
            or uint
            or long:
                builder.Append(
                    Convert
                        .ToInt64(value, CultureInfo.InvariantCulture)
                        .ToString(CultureInfo.InvariantCulture)
                );
                break;
            case ulong u:
                builder.Append(u.ToString(CultureInfo.InvariantCulture));
                break;
            default:
                throw new SerializationException(
                    "Value of type '"
                        + value.GetType().Name
                        + "' is not supported by the GeoJSON serializer"
                );
        }
    }
}
