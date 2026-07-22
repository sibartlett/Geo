# Geodesy — distance, bearing, and lines

Geo computes distances and bearings on the surface of the Earth (not on a flat
projection). The calculations are exposed as extension methods on any position
(`Coordinate`, `Point`, `Waypoint`, … — anything implementing `IPosition`) and
return strongly-typed results.

```csharp
using Geo;
using Geo.Geodesy;
using Geo.Measure;

// Ordinate order is latitude, longitude.
var london = new Coordinate(51.5074, -0.1278);
var newYork = new Coordinate(40.7128, -74.0060);

GeodeticLine line = london.CalculateShortestLine(newYork);

Distance distance = line.Distance;          // a Distance (S.I.: metres)
double km = distance.ConvertTo(DistanceUnit.Km).Value;   // ~5570
double initialBearing = line.Bearing12;     // degrees, London -> New York
double finalBearing = line.Bearing21;       // degrees, New York -> London
```

## Great-circle vs. rhumb lines

| Method | Line | Notes |
|--------|------|-------|
| `CalculateShortestLine` | Geodesic (shortest path) | Same as `CalculateGreatCircleLine` |
| `CalculateGreatCircleLine` | Great circle / orthodromic | Shortest path; bearing changes along the route |
| `CalculateRhumbLine` | Rhumb line / loxodrome | Constant bearing; longer than the great circle |

```csharp
var greatCircle = london.CalculateGreatCircleLine(newYork);
var rhumb = london.CalculateRhumbLine(newYork);
// rhumb.Distance >= greatCircle.Distance
```

A `GeodeticLine` is a `LineSegment`, so it also carries its two end
coordinates. The `Distance` and bearings are the interesting additions.

## Projecting a point

Given a start point, a heading, and a distance, the calculator can project the
destination point:

```csharp
using Geo;
using Geo.Geodesy;

var start = new Coordinate(51.5074, -0.1278);
GeodeticLine projected = GeoContext.Current.GeodeticCalculator
    .CalculateOrthodromicLine(start, heading: 90, distance: 10000); // 10 km due east
// projected.Coordinate2 is the destination
```

## Lengths and areas

The active calculator computes lengths and areas for whole shapes:

```csharp
var calculator = GeoContext.Current.GeodeticCalculator;

Distance perimeter = calculator.CalculateLength(polygon.Shell.Coordinates);
Area area = calculator.CalculateArea(polygon.Shell.Coordinates);
Distance circumference = calculator.CalculateLength(circle);
```

## Choosing the Earth model

Calculations read their calculator from `GeoContext.Current.GeodeticCalculator`.
Two implementations are provided:

- **`SpheroidCalculator`** — models the Earth as an ellipsoid (spheroid). This is
  the default, using the WGS-84 spheroid, and is the more accurate option.
- **`SphereCalculator`** — models the Earth as a sphere. Faster and simpler, but
  less accurate.

```csharp
using Geo;
using Geo.Geodesy;

// Use a spherical Earth model globally:
GeoContext.Current.GeodeticCalculator = new SphereCalculator();

// Or a different reference spheroid:
GeoContext.Current.GeodeticCalculator = new SpheroidCalculator(Spheroid.Grs80);
```

Built-in spheroids include `Spheroid.Wgs84` (the default), `Spheroid.Grs80`,
`Spheroid.International1924`, and `Spheroid.Clarke1866`. You can also construct a
custom one with `new Spheroid(name, equatorialAxis, inverseFlattening)`.

The `Distance` and `Area` result types (from the `Geo.Measure` namespace) carry
their value in S.I. units and convert to others via `ConvertTo`, e.g.
`distance.ConvertTo(DistanceUnit.Nm).Value`.
