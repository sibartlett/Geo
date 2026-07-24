using System.Collections.Generic;
using System.Runtime.Serialization;
using Geo.IO.GeoJson;
using Xunit;

namespace Geo.Tests.IO.GeoJson;

public class JsonTests
{
    // ---- Parsing: primitives ------------------------------------------------

    [Fact]
    public void Null_input_parses_to_null_without_failing()
    {
        Assert.True(Json.TryParse(null, out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("true", true)]
    [InlineData("false", false)]
    [InlineData("  true  ", true)]
    public void Boolean_literals_parse(string json, bool expected)
    {
        Assert.True(Json.TryParse(json, out var result));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Null_literal_parses_to_null()
    {
        Assert.True(Json.TryParse("null", out var result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData("0", 0L)]
    [InlineData("42", 42L)]
    [InlineData("-7", -7L)]
    [InlineData("  123  ", 123L)]
    public void Integers_parse_as_long(string json, long expected)
    {
        Assert.True(Json.TryParse(json, out var result));
        Assert.IsType<long>(result);
        Assert.Equal(expected, result);
    }

    [Theory]
    [InlineData("1.5", 1.5)]
    [InlineData("-3.25", -3.25)]
    [InlineData("1e3", 1000d)]
    [InlineData("6.02E23", 6.02e23)]
    public void Reals_parse_as_double(string json, double expected)
    {
        Assert.True(Json.TryParse(json, out var result));
        Assert.IsType<double>(result);
        Assert.Equal(expected, (double)result!);
    }

    // ---- Parsing: strings & escapes ----------------------------------------

    [Fact]
    public void Plain_string_parses()
    {
        Assert.True(Json.TryParse("\"hello\"", out var result));
        Assert.Equal("hello", result);
    }

    [Theory]
    [InlineData("\"a\\\"b\"", "a\"b")] // \"
    [InlineData("\"a\\\\b\"", "a\\b")] // \\
    [InlineData("\"a\\/b\"", "a/b")] // \/
    [InlineData("\"a\\bb\"", "a\bb")] // \b
    [InlineData("\"a\\fb\"", "a\fb")] // \f
    [InlineData("\"a\\nb\"", "a\nb")] // \n
    [InlineData("\"a\\rb\"", "a\rb")] // \r
    [InlineData("\"a\\tb\"", "a\tb")] // \t
    public void Escape_sequences_are_decoded(string json, string expected)
    {
        Assert.True(Json.TryParse(json, out var result));
        Assert.Equal(expected, result);
    }

    [Fact]
    public void Unicode_escape_is_decoded()
    {
        Assert.True(Json.TryParse("\"\\u0041\\u00e9\"", out var result));
        Assert.Equal("Aé", result);
    }

    [Fact]
    public void Surrogate_pair_escapes_are_reassembled()
    {
        // U+1F600 GRINNING FACE, encoded as a UTF-16 surrogate pair.
        Assert.True(Json.TryParse("\"\\uD83D\\uDE00\"", out var result));
        Assert.Equal("\uD83D\uDE00", result);
    }

    // ---- Parsing: objects & arrays -----------------------------------------

    [Fact]
    public void Empty_object_parses()
    {
        Assert.True(Json.TryParse("{}", out var result));
        var obj = Assert.IsType<JsonObject>(result);
        Assert.Empty(obj);
    }

    [Fact]
    public void Empty_array_parses()
    {
        Assert.True(Json.TryParse("[]", out var result));
        var arr = Assert.IsType<JsonArray>(result);
        Assert.Empty(arr);
    }

    [Fact]
    public void Nested_structure_parses()
    {
        Assert.True(
            Json.TryParse(
                "{ \"a\" : 1, \"b\" : [true, null, \"x\"], \"c\" : { \"d\" : 2.5 } }",
                out var result
            )
        );

        var obj = Assert.IsType<JsonObject>(result);
        Assert.Equal(1L, obj["a"]);

        var array = Assert.IsType<JsonArray>(obj["b"]);
        Assert.Equal(new object?[] { true, null, "x" }, array);

        var inner = Assert.IsType<JsonObject>(obj["c"]);
        Assert.Equal(2.5, inner["d"]);
    }

    // ---- Parsing: malformed input fails ------------------------------------

    [Theory]
    [InlineData("")] // empty
    [InlineData("   ")] // whitespace only
    [InlineData("not valid")] // bad literal
    [InlineData("tru")] // truncated true
    [InlineData("nul")] // truncated null
    [InlineData("\"unterminated")] // unterminated string
    [InlineData("\"bad\\x\"")] // invalid escape
    [InlineData("\"\\u12\"")] // truncated unicode escape
    [InlineData("\"\\uZZZZ\"")] // non-hex unicode escape
    [InlineData("{")] // truncated object
    [InlineData("{\"a\"}")] // missing colon
    [InlineData("{\"a\":}")] // missing value
    [InlineData("[")] // truncated array
    [InlineData("[1,")] // truncated after comma
    public void Malformed_json_fails_to_parse(string json)
    {
        Assert.False(Json.TryParse(json, out _));
    }

    // ---- Serialization: primitives -----------------------------------------

    [Fact]
    public void Serializes_null()
    {
        Assert.Equal("null", Json.Serialize(null));
    }

    [Theory]
    [InlineData(true, "true")]
    [InlineData(false, "false")]
    public void Serializes_booleans(bool value, string expected)
    {
        Assert.Equal(expected, Json.Serialize(value));
    }

    [Fact]
    public void Serializes_doubles_round_trip()
    {
        Assert.Equal("0", Json.Serialize(0d));
        Assert.Equal("1.5", Json.Serialize(1.5d));
        Assert.Equal("-87.93939", Json.Serialize(-87.93939d));
    }

    [Fact]
    public void Serializes_all_supported_numeric_types()
    {
        Assert.Equal("42", Json.Serialize(42L));
        Assert.Equal("-7", Json.Serialize(-7));
        Assert.Equal("255", Json.Serialize((byte)255));
        Assert.Equal("7", Json.Serialize((short)7));
        Assert.Equal("7", Json.Serialize((ushort)7));
        Assert.Equal("7", Json.Serialize((uint)7));
        Assert.Equal("18446744073709551615", Json.Serialize(ulong.MaxValue));
        Assert.Equal("1.5", Json.Serialize(1.5f));
        Assert.Equal("1.5", Json.Serialize(1.5m));
    }

    [Fact]
    public void Serializes_string_with_escapes()
    {
        Assert.Equal("\"a\\\"b\\\\c\\n\\t\"", Json.Serialize("a\"b\\c\n\t"));
    }

    [Fact]
    public void Serializes_control_characters_as_unicode_escapes()
    {
        Assert.Equal("\"\\u0001\"", Json.Serialize("\u0001"));
    }

    [Fact]
    public void Unsupported_value_type_throws_serialization()
    {
        Assert.Throws<SerializationException>(() => Json.Serialize(new object()));
    }

    // ---- Serialization: objects & arrays -----------------------------------

    [Fact]
    public void Serializes_dictionary()
    {
        var dictionary = new Dictionary<string, object?>
        {
            { "name", "test" },
            { "count", 3L },
            { "flag", true },
            { "missing", null },
        };

        Assert.Equal(
            "{\"name\":\"test\",\"count\":3,\"flag\":true,\"missing\":null}",
            Json.Serialize(dictionary)
        );
    }

    [Fact]
    public void Serializes_array()
    {
        Assert.Equal("[1,\"a\",true,null]", Json.Serialize(new object?[] { 1L, "a", true, null }));
    }

    [Fact]
    public void Serializes_nested_structure()
    {
        var value = new Dictionary<string, object?>
        {
            { "items", new object?[] { 1L, 2L } },
            {
                "child",
                new Dictionary<string, object?> { { "x", 1.5d } }
            },
        };

        Assert.Equal("{\"items\":[1,2],\"child\":{\"x\":1.5}}", Json.Serialize(value));
    }

    // ---- Round-trip ---------------------------------------------------------

    [Fact]
    public void Parse_then_serialize_round_trips()
    {
        const string json = "{\"type\":\"Point\",\"coordinates\":[1,2.5],\"ok\":true}";
        Assert.True(Json.TryParse(json, out var parsed));
        Assert.Equal(json, Json.Serialize(parsed));
    }
}
