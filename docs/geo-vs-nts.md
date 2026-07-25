# Geo vs. NetTopologySuite

[NetTopologySuite](https://github.com/NetTopologySuite/NetTopologySuite) (NTS) is
the most widely used spatial library for .NET, so it's a natural point of
comparison. This page explains where Geo and NTS differ and when to reach for
each — they solve overlapping but genuinely different problems.

## The fundamental difference

**NetTopologySuite is a planar geometry engine.** It's the .NET port of the
JTS Topology Suite, and its model assumes a flat Cartesian plane. Its strengths
are *topological operations*: intersection, union, difference, buffering, convex
hulls, `DE-9IM` spatial predicates (`Intersects`, `Contains`, `Within`,
`Overlaps`), spatial indexing (STRtree, quadtree), overlay, noding, and validity
checking. Because the model is planar, distances and areas come out in the units
of the coordinates you give it — so if you feed it lat/lon degrees, the results
are in *degrees*, which are meaningless on the ground. You're expected to project
to a planar coordinate system first (with ProjNet or similar).

**Geo is a geographic library.** It assumes the surface of the Earth (WGS-84)
and works in real-world units. Its distance, bearing, and area calculations are
*geodetic* — great-circle and rhumb lines computed on a spheroid (the default
[`SpheroidCalculator`](geodesy.md)) or a sphere — and they return strongly-typed
`Distance` / `Area` values in S.I. units. No projection step is required.

## When to choose Geo

- **You need correct geographic math out of the box.**
  `london.CalculateShortestLine(newYork)` yields ~5570 km on a WGS-84 spheroid.
  The equivalent in NTS means choosing and managing a projection and pulling in
  ProjNet. Geo also does the inverse — project a destination point from a start
  point, heading, and distance. See the [geodesy guide](geodesy.md).
- **You want strongly-typed measurements.** `Distance`, `Area`, and `Speed` with
  unit enums and `ConvertTo` (km, nautical miles, etc.). NTS hands back bare
  `double`s in whatever unit your coordinates happened to be in. See the
  [measure guide](measure.md).
- **You work with GPS or aviation data.** Readers for GPX (1.0/1.1, read and
  write), NMEA, IGC, and Garmin / PocketFMS / SkyDemon flightplans, plus
  `GpsData` / `Route` / `Track` / `Waypoint` types. NTS has none of this. See the
  [GPS guide](gps.md).
- **You need geomagnetism.** IGRF and WMM models through 2025 — declination,
  inclination, and intensity. This is entirely outside NTS's scope. See the
  [geomagnetism guide](geomagnetism.md).
- **You want lat/lon-first ergonomics.** Constructors are `(lat, lon)`,
  everything defaults to degrees and WGS-84, `CoordinateZ` / `CoordinateM` /
  `CoordinateZM` cover elevation and measures, and `GeoFormat.Detect` sniffs
  coordinate pairs, WKT, and GeoJSON.
- **You want a small, dependency-free library.** Geo targets netstandard2.0 and
  ships with no external dependencies (even its JSON parsing is vendored).

## When to choose NetTopologySuite

- **You need topology / geometry algebra** — intersections, unions, differences,
  buffers, spatial predicates, validity checking, or simplification. Geo does not
  provide these.
- **You need spatial indexing** for querying large sets of geometries.
- **You work with projected / planar data**, or you integrate with **EF Core
  spatial, GDAL, PostGIS, or SQL Server spatial** — all of which speak NTS. Its
  interop and ecosystem are far deeper.
- **You want the battle-tested, widely-adopted engine** with a large community
  behind it.

## They're complementary

This isn't strictly either/or. It's reasonable to use NTS for topology and Geo
for the geodetic, GPS, and geomagnetic work it is purpose-built for. Geo speaks
[WKT](well-known-text.md), [WKB](well-known-binary.md), and
[GeoJSON](geojson.md), so moving geometries between the two libraries is
straightforward.

## Summary

| | **Geo** | **NetTopologySuite** |
|---|---|---|
| Model | Geographic (Earth's surface, WGS-84) | Planar (Cartesian) |
| Distances / areas | Geodetic, in S.I. units | Planar, in coordinate units |
| Topology (intersect, union, buffer, predicates) | — | ✓ |
| Spatial indexing | — | ✓ |
| GPS / flightplan formats | ✓ | — |
| Geomagnetism (IGRF / WMM) | ✓ | — |
| WKT / WKB / GeoJSON | ✓ | ✓ |
| Database / GDAL / EF Core interop | — | ✓ |
| Dependencies | None (netstandard2.0) | Several, plus ecosystem packages |

**Rule of thumb:** reach for **Geo** when the problem is *"points on the
Earth"* — distances, bearings, GPS tracks, flight plans, magnetic declination,
real-world units. Reach for **NetTopologySuite** when the problem is *"geometry
algebra"* — computing intersections, buffers, and spatial relationships,
especially against a spatial database.
