# Envelope

The `Envelope` type represents a rectangle/bounds. It has four properties:

- `MinLat`
- `MinLon`
- `MaxLat`
- `MaxLon`

The horizontal edges run along parallels; the vertical edges run along
meridians.

```csharp
var envelope = new Envelope(minLat: 0, minLon: 0, maxLat: 10, maxLon: 10);
```

## Calculations

`Envelope` supports several calculations:

- `Contains(Coordinate other)`
- `Contains(Envelope other)`
- `Contains(IGeometry other)`
- `GetArea()` — returns an `Area`
- `GetLength()` — returns a `Distance` (the perimeter)
- `Intersects(Envelope other)`
