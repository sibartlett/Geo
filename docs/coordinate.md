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

The hemisphere letter may lead the ordinate as well as follow it, which is how
aviation and marine sources usually write it:

```csharp
Coordinate.Parse("N51 30.0, W000 07.2");        // 51.5, -0.12
Coordinate.Parse("N51°30.0', W000°07.2'");      // 51.5, -0.12
Coordinate.Parse("S33 52 00, E151 12 00");      // -33.8666…, 151.2
```

The ordinates are always read **latitude first**, so the letters have to agree
with that order: an `E` or a `W` on the first ordinate (or an `N`/`S` on the
second) fails the parse rather than being ignored. A letter on both sides of one
ordinate is allowed as long as it says the same thing twice.

```csharp
Coordinate.TryParse("W000 07.2, N51 30.0", out var swapped);  // false — longitude first
```

Degrees, minutes and seconds may be separated by a degree/minute/second mark,
whitespace, or a hyphen — the last being the form the FAA and the NGS use:

```csharp
Coordinate.Parse("40-26-46N, 079-56-55W");      // 40.4461…, -79.9486…
Coordinate.Parse("N51-30-00, W000-07-12");      // 51.5, -0.12
```

Only the degrees carry a sign; minutes and seconds are magnitudes, so a hyphen in
front of one is always the separator introducing it and never a negative value.
Whichever separator is used, it has to be there: `1+2, 3` and `1.2.3, 4` do not
parse, because the second field is not introduced by anything.

```csharp
Coordinate.Parse("51 -30, 0");                  // 51.5 — 51° 30′, not 51° less 30′
Coordinate.Parse("-0 07 12, 10");               // -0.12 — the sign is on the degrees
```

If you don't know whether a string is a coordinate pair, WKT, or GeoJSON, use
[`GeoFormat`](parsing.md) to detect the format and parse it in one step.
