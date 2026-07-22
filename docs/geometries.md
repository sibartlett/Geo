# Geometries

Geo's geometry types mirror the OGC simple-feature geometries, plus a `Circle`.
All coordinates are latitude/longitude in degrees (WGS-84), and constructors take
them in **latitude, longitude** order.

```csharp
using Geo;
using Geo.Geometries;
```

## Point

A single position. Similar to the OGC Point.

```csharp
var point = new Point(51.5074, -0.1278);          // lat, lon
var fromCoordinate = new Point(new Coordinate(51.5074, -0.1278));
```

## LineString

An ordered sequence of coordinates. Similar to the OGC LineString.

```csharp
var line = new LineString(
    new Coordinate(0, 0),
    new Coordinate(0, 10),
    new Coordinate(10, 10)
);
// also accepts IEnumerable<Coordinate> or a CoordinateSequence
```

## LinearRing

A closed `LineString` whose first and last coordinates are equal. Similar to the
OGC LinearRing. (A plain `LineString` may happen to be closed too, but it is not
necessarily *semantically* a ring.)

```csharp
var ring = new LinearRing(
    new Coordinate(0, 0),
    new Coordinate(0, 10),
    new Coordinate(10, 10),
    new Coordinate(0, 0)   // closes the ring
);
```

## Polygon

An exterior ring (shell) plus zero or more interior rings (holes). Similar to
the OGC Polygon.

```csharp
// Simple polygon from a coordinate list (the shell):
var polygon = new Polygon(
    new Coordinate(0, 0),
    new Coordinate(0, 10),
    new Coordinate(10, 10),
    new Coordinate(0, 0)
);

// Polygon with a hole:
var withHole = new Polygon(shell: outerRing, holes: innerRing);
```

Though it is not enforced, it is advised that the exterior ring is CCW
(counter-clockwise) and the interior rings/holes are CW (clockwise).

## Triangle

A `Polygon` whose shell is limited to 3 distinct points. Similar to the OGC
Triangle.

```csharp
var triangle = new Triangle(
    new Coordinate(0, 0),
    new Coordinate(0, 10),
    new Coordinate(10, 0)
);
```

## Circle

A centre and a radius **in metres**. Does not map to an OGC-defined shape.

```csharp
var circle = new Circle(new Coordinate(51.5074, -0.1278), radius: 5000); // 5 km
var circle2 = new Circle(51.5074, -0.1278, 5000);                        // lat, lon, radius
```

## Collections

`GeometryCollection` holds any mix of geometries; the `Multi*` types hold a
single geometry kind. Each accepts a `params` array or an `IEnumerable`.

```csharp
var multiPoint = new MultiPoint(
    new Point(0, 0),
    new Point(0, 10)
);

var multiLine = new MultiLineString(line1, line2);
var multiPolygon = new MultiPolygon(polygon1, polygon2);
var collection = new GeometryCollection(point, line, polygon);
```

- `MultiPoint` — similar to the OGC MultiPoint.
- `MultiLineString` — similar to the OGC MultiLineString.
- `MultiPolygon` — similar to the OGC MultiPolygon.
- `GeometryCollection` — similar to the OGC GeometryCollection.

See also: [WKT](well-known-text.md), [WKB](well-known-binary.md), and
[GeoJSON](geojson.md) for reading and writing these geometries.
