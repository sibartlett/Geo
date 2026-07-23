# Well-Known Text (WKT)

Geo can read and write WKT (Well-Known Text). Geometries that implement
`IGeometry` can be read from and written to WKT.

## Reading

```csharp
var reader = new WktReader();
IGeometry point = reader.Read("POINT (73.89 68.389)");   // reading a string
IGeometry geometry = reader.Read(myStream);              // reading a stream
```

`ReadAsync` reads the stream asynchronously (with an optional
`CancellationToken`) before parsing:

```csharp
IGeometry geometry = await new WktReader().ReadAsync(myStream);
```

## Writing

```csharp
var writer = new WktWriter();
var pointString = writer.Write(new Point(68.389, 73.89));
```

A number of settings are available to customise the output. The code below shows
their default values:

```csharp
var settings = new WktWriterSettings
{
    LinearRing = false,
    Triangle = false,
    DimensionFlag = true,
    ConvertCirclesToRegularPolygons = false,
};

var writer = new WktWriter(settings); // pass the settings into the writer's constructor
```

There is also a static factory for settings that are compatible with NTS/JTS:

```csharp
var writer = new WktWriter(WktWriterSettings.NtsCompatible);
```
