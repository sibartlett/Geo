# Well-Known Binary (WKB)

Geo can read and write WKB (Well-Known Binary). Geometries that implement
`IGeometry` can be read from and written to WKB.

## Reading

```csharp
var reader = new WkbReader();
IGeometry geometry1 = reader.Read(new byte[] { /* ... */ }); // reading a byte array
IGeometry geometry2 = reader.Read(myStream);                 // reading a stream
```

## Writing

```csharp
var writer = new WkbWriter();
byte[] bytes = writer.Write(new Point(68.389, 73.89));
```

A number of settings are available to customise the output. The code below shows
their default values:

```csharp
var settings = new WkbWriterSettings
{
    Encoding = WkbEncoding.LittleEndian,
    Triangle = false,
    MaxDimesions = 4,
    ConvertCirclesToRegularPolygons = false,
    CircleSides = 36,
};

var writer = new WkbWriter(settings); // pass the settings into the writer's constructor
```

There is also a static factory for settings that are compatible with NTS/JTS:

```csharp
var writer = new WkbWriter(WkbWriterSettings.NtsCompatible);
```
