# Geomagnetism

Geo computes the Earth's magnetic field at a given position and date using the
standard spherical-harmonic models — the **World Magnetic Model (WMM)** and the
**International Geomagnetic Reference Field (IGRF)**. This gives you magnetic
declination (variation), inclination (dip), and field intensity.

## Calculating declination

Pick a model calculator, then call `TryCalculate` with a position and a date:

```csharp
using System;
using Geo;
using Geo.Geomagnetism;

var london = new Coordinate(51.5, -0.12);

var result = new WmmGeomagnetismCalculator()
    .TryCalculate(london, new DateTime(2020, 1, 1));

double declination = result.Declination;    // degrees, east positive
double inclination = result.Inclination;    // degrees
double intensity = result.TotalIntensity;   // nanoteslas
```

Two calculators are provided:

- **`WmmGeomagnetismCalculator`** — the WMM models (epochs 1985 through 2025).
- **`IgrfGeomagnetismCalculator`** — the IGRF models.

Both come pre-loaded with their full set of epoch models and pick the correct
one for the date you pass.

## The result

`TryCalculate(position, date)` returns a `GeomagnetismResult` with:

| Property | Meaning |
|----------|---------|
| `Declination` | Angle between magnetic and true north, in degrees (east positive) |
| `Inclination` | Dip angle, in degrees |
| `TotalIntensity` | Total field strength, in nanoteslas |
| `HorizontalIntensity` | Horizontal field strength, in nanoteslas |
| `X`, `Y`, `Z` | North, east, and downward field components (see below) |

### Component coordinate frame

`X`, `Y`, and `Z` are given in the **NED (North, East, Down)** geodetic
reference frame used by the World Magnetic Model — `X` points north, `Y`
points east, and `Z` points down (positive towards the centre of the Earth),
all in nanoteslas.

If you are working with device sensors that use an **ENU (East, North, Up)**
frame (as Android and iOS do), convert with:

```csharp
double east = result.Y;
double north = result.X;
double up = -result.Z;
```

There are also `out`-parameter overloads that return a `bool` indicating
success:

```csharp
if (new WmmGeomagnetismCalculator().TryCalculate(london, DateTime.UtcNow, out var r))
{
    Console.WriteLine($"Declination: {r.Declination:F2}°");
}
```

## Elevation and dates

- Position elevation is taken into account when you pass a `CoordinateZ`
  (elevation in metres); a plain `Coordinate` is treated as sea level.
- Dates are handled in UTC — `DateTimeOffset` overloads are available and are
  converted to UTC internally.

## Custom models

Both calculators derive from `GeomagnetismCalculator`, which you can construct
with your own set of `IGeomagneticModel` instances (and optionally a specific
`Spheroid`) if you need to restrict or extend the available epochs.
