# GeoContext — ambient settings

`GeoContext` holds the settings that calculations read by default — the
reference spheroid, the geodetic and geomagnetic calculators, spatial-equality
options, and the longitude-wrapping flag. The active context is
`GeoContext.Current`.

```csharp
using Geo;

var context = GeoContext.Current;
```

## Settings

| Property | Type | Default |
|----------|------|---------|
| `Spheroid` | `Spheroid` | `Spheroid.Wgs84` |
| `GeodeticCalculator` | `IGeodeticCalculator` | `SpheroidCalculator` (WGS-84) |
| `GeomagnetismCalculator` | `GeomagnetismCalculator` | `GeomagnetismCalculator` |
| `EqualityOptions` | `SpatialEqualityOptions` | default options |
| `LongitudeWrapping` | `bool` | `false` |

## Changing the defaults

You can set individual properties on the current context:

```csharp
using Geo;
using Geo.Geodesy;

// Use a spherical Earth model for geodetic calculations:
GeoContext.Current.GeodeticCalculator = new SphereCalculator();

// Enable longitude wrapping (see the Coordinate page):
GeoContext.Current.LongitudeWrapping = true;
```

…or replace the whole context, optionally seeding it from a specific spheroid
(which sets up matching geodetic and geomagnetic calculators):

```csharp
using Geo;
using Geo.Geodesy;

GeoContext.Current = new GeoContext(Spheroid.Grs80);
```

## Spatial equality options

`EqualityOptions` controls how geometries compare for *spatial* equality
(the `Equals(object, SpatialEqualityOptions)` overloads). Notable flags:

- `UseElevation` — include the Z ordinate in comparisons.
- `UseM` — include the measure ordinate.
- `PoleCoordiantesAreEqual` — treat all coordinates at a pole as equal.
- `AntiMeridianCoordinatesAreEqual` — treat longitude `-180` and `+180` as equal.

The `To2D()` and `To3D()` helpers return option sets tuned for 2D or 3D
comparison.

## Scope

`GeoContext.Current` is process-wide and mutable — changing it affects every
calculation that reads from it. Treat it as global configuration you set up
once at start-up, and be mindful when mutating it from multiple threads.

See also: [Geodesy](geodesy.md), [Geomagnetism](geomagnetism.md), and
[Coordinate](coordinate.md) for the features it configures.
