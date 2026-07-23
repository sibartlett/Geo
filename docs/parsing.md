# Detecting and parsing geo strings

Sometimes you have a geospatial string but don't know its format up front — it
might be a coordinate pair, Well-Known Text, or GeoJSON. `GeoFormat` (in the
`Geo.IO` namespace) detects the format and parses the string in one step.

```csharp
using Geo.IO;

GeoStringFormat format = GeoFormat.Detect("POINT (30 10)");   // GeoStringFormat.Wkt
```

The supported formats are reported by the `GeoStringFormat` enum:

| Value | Example input |
|-------|---------------|
| `GeoStringFormat.Coordinate` | `42.29, -89.63`, `50°03'46"S 125°48'26"E` |
| `GeoStringFormat.Wkt` | `POINT (30 10)` |
| `GeoStringFormat.GeoJson` | `{ "type": "Point", "coordinates": [30, 10] }` |
| `GeoStringFormat.Unknown` | anything unrecognised |

## Parsing

`TryParse` returns both the parsed value and the format it detected:

```csharp
if (GeoFormat.TryParse(input, out object result, out GeoStringFormat format))
{
    switch (format)
    {
        case GeoStringFormat.Coordinate:
            var coordinate = (Coordinate)result;
            break;
        case GeoStringFormat.Wkt:
            var geometry = (Geo.Abstractions.Interfaces.IGeometry)result;
            break;
        case GeoStringFormat.GeoJson:
            var geoJson = (Geo.Abstractions.Interfaces.IGeoJsonObject)result;
            break;
    }
}
```

The returned type depends on the detected format: a `Coordinate` for a
coordinate string, an `IGeometry` for WKT, and an `IGeoJsonObject` for GeoJSON.

A throwing variant is also available:

```csharp
object result = GeoFormat.Parse(input);   // throws FormatException on an unknown format
```

`Parse` throws `ArgumentNullException` for a `null` input and `FormatException`
when the string matches no supported format.

## How detection works

`GeoFormat` delegates to the existing [`Coordinate`](coordinate.md) parser,
[`WktReader`](well-known-text.md), and [`GeoJsonReader`](geojson.md), so it never
duplicates parsing logic and `Detect` always agrees with `TryParse`. A string
that begins with `{` could be either GeoJSON or a braced coordinate pair such as
`{42.29, -89.63}`; GeoFormat tries GeoJSON first and falls back to a coordinate
parse.

Detection covers textual formats only. Well-Known Binary is binary, not a
string, so parse it directly with [`WkbReader`](well-known-binary.md).
