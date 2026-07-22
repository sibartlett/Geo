# Coordinate

The `Coordinate` type represents a geographic position as a latitude/longitude
pair (in degrees, WGS-84):

- `Latitude`
- `Longitude`

```csharp
var latitude = 34.869;
var longitude = 67.98;
var coordinate = new Coordinate(latitude, longitude);
```

## Elevation and measure

`Coordinate` itself only carries latitude and longitude. Elevation and measure
values live on the derived coordinate types:

| Type | Adds |
|------|------|
| `Coordinate` | latitude, longitude |
| `CoordinateZ` | + elevation (metres) |
| `CoordinateM` | + measure |
| `CoordinateZM` | + elevation and measure |

```csharp
var withElevation = new CoordinateZ(34.869, 67.98, 789.93);
```

Geo does not perform geodetic calculations on the elevation or measure
ordinates. A coordinate's `Is3D` and `IsMeasured` properties report which
variant you are holding.

## Constraints

The constructor throws an `ArgumentOutOfRangeException` if:

- Latitude is greater than `90` or less than `-90`.
- Longitude is greater than `180` or less than `-180`.

## Longitude wrapping

`Coordinate` supports longitude wrapping. When enabled, Geo wraps out-of-range
longitude values into the `[-180, 180]` range instead of throwing — for example
a longitude of `390` wraps to `30`:

```csharp
GeoContext.Current.LongitudeWrapping = true;

var coordinate1 = new Coordinate(0, 30);
var coordinate2 = new Coordinate(0, 390);

// coordinate2.Longitude == 30, so the two coordinates are equal
Assert.Equal(coordinate1, coordinate2);
Assert.Equal(30d, coordinate2.Longitude);
```

Latitude is never wrapped; an out-of-range latitude always throws.

## Parsing coordinate strings

`Coordinate` has static methods for parsing a coordinate pair from a string,
including degrees/minutes/seconds notation:

```csharp
var coordinate = Coordinate.Parse("12 34.56'N 123 45.55'E");

// Non-throwing variants:
var maybe = Coordinate.TryParse("12 34.56'N 123 45.55'E");     // returns null on failure
if (Coordinate.TryParse("...", out var parsed)) { /* ... */ }
```
