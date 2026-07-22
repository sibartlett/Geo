# GeoJSON

Geo can read and write GeoJSON. Objects that implement `IGeoJsonObject` can be
read from and written to GeoJSON.

There are two types specific to GeoJSON:

- `Feature`
- `FeatureCollection`

## Reading

```csharp
var reader = new GeoJsonReader();
IGeoJsonObject point = reader.Read("{ \"type\": \"Point\", \"coordinates\": [100.0, 0.0] }");
```

## Writing

```csharp
var writer = new GeoJsonWriter();
var pointJson = writer.Write(new Point(68.389, 73.89));
```

`GeoJsonWriter.Write` has overloads for `IGeometry`, `Feature`, and
`FeatureCollection`.

Writing anything that implements `IGeometry` works the same way — including
geometries produced from GPS data. To convert a GPX track (or route) to GeoJSON,
see the [GPX to GeoJSON recipe](gps.md#recipe-gpx-to-geojson).
