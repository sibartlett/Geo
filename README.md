# Geo — a geospatial library for .NET

[![NuGet](https://img.shields.io/nuget/v/Geo.svg)](https://nuget.org/packages/Geo)
[![Downloads](https://img.shields.io/nuget/dt/Geo.svg)](https://nuget.org/packages/Geo)
[![License: LGPL-3.0-or-later](https://img.shields.io/badge/license-LGPL--3.0--or--later-blue.svg)](LICENSE)

**Geo** is a spatial library for .NET built specifically for *geographic* data —
coordinates on the surface of the Earth — rather than planar/projected data. It
provides geometry and GPS types, serializers for common spatial and GPS file
formats, and geodetic and geomagnetic calculations.

[Documentation](docs/) · [Issues](https://github.com/sibartlett/Geo/issues) · [NuGet](https://nuget.org/packages/Geo)

## Contents

- [Installation](#installation)
- [Quick start](#quick-start)
- [Features](#features)
- [Conventions](#conventions)
- [Building from source](#building-from-source)
- [Contributing](#contributing)
- [License](#license)

## Installation

Geo targets **.NET Standard 2.0**, so it runs on .NET 5+, .NET Core 2.0+, and
.NET Framework 4.6.1+. Install it from [NuGet](https://nuget.org/packages/Geo):

```bash
dotnet add package Geo
```

## Quick start

```csharp
using Geo;
using Geo.Geometries;
using Geo.Geodesy;
using Geo.IO.Wkt;
using Geo.IO.GeoJson;
using Geo.Measure;

// Ordinate order is latitude, longitude — all ordinates are in degrees.
var london = new Coordinate(51.5074, -0.1278);
var newYork = new Coordinate(40.7128, -74.0060);

// Distance and bearing along the great-circle (shortest) line.
GeodeticLine line = london.CalculateShortestLine(newYork);
double km = line.Distance.ConvertTo(DistanceUnit.Km).Value;   // ~5570 km
double initialBearing = line.Bearing12;                       // degrees

// Serialize a geometry to WKT and GeoJSON.
var point = new Point(london);
string wkt = new WktWriter().Write(point);                    // "POINT (-0.1278 51.5074)"
string geoJson = new GeoJsonWriter().Write(point);

// Parse it back from WKT.
IGeometry parsed = new WktReader().Read(wkt);
```

## Features

### Geometry types

- `Point`
- `LineString`, `LinearRing`
- `Polygon`, `Triangle`
- `Circle`
- `GeometryCollection`, `MultiPoint`, `MultiLineString`, `MultiPolygon`

Coordinates come in four flavours: `Coordinate` (lat/lon), `CoordinateZ`
(with elevation), `CoordinateM` (with a measure), and `CoordinateZM` (both).

### GPS types

- `GpsData`, `Route`, `Track`, `Waypoint`

### Serialize / deserialize geometries

| Format | Read | Write |
|--------|:----:|:-----:|
| WKT (Well-Known Text)   | ✓ | ✓ |
| WKB (Well-Known Binary) | ✓ | ✓ |
| GeoJSON                 | ✓ | ✓ |

Don't know a string's format up front? `GeoFormat.Detect` / `GeoFormat.TryParse`
sniff whether it is a coordinate pair, WKT, or GeoJSON and parse it — see the
[parsing guide](docs/parsing.md).

### Serialize / deserialize GPS files

| Format | Read | Write |
|--------|:----:|:-----:|
| GPX (1.0 & 1.1)         | ✓ | ✓ |
| NMEA                    | ✓ |   |
| IGC                     | ✓ |   |
| Garmin flightplan       | ✓ |   |
| SkyDemon flightplan     | ✓ |   |
| PocketFMS flightplan    | ✓ |   |

### Geographic calculations

- Distance and bearing
- Area
- Great-circle lines
- Rhumb lines

### Geomagnetism calculations

- IGRF and WMM models (through 2025)
- Declination, inclination, intensity, and more

## Conventions

- All ordinates are in **degrees**, unless specified otherwise.
- All measurements are in **S.I. units** (metres, seconds, etc.), unless specified otherwise.
- The coordinate reference system is assumed to be **WGS-84**.
- Ordinate order in constructors is **latitude, longitude** (e.g. `new Coordinate(lat, lon)`).

## Building from source

The project uses [NUKE](https://nuke.build/) for build automation. Use the
bootstrap scripts, which pin the SDK and run the same targets as CI:

```bash
./build.sh            # compile (default target)
./build.sh Test       # build and run tests
```

On Windows, use `build.cmd` / `build.ps1`. See [AGENTS.md](AGENTS.md) for a full
description of the repository layout, build targets, and coding conventions.

## Contributing

Issues and pull requests are welcome. Formatting is enforced by
[CSharpier](https://csharpier.com/) and checked in CI — run
`dotnet csharpier format .` before committing (a pre-commit hook does this for
staged files). New library code must remain compatible with **.NET Standard 2.0**.

## License

Geo is licensed under the terms of the **GNU Lesser General Public License,
version 3 or later** (`LGPL-3.0-or-later`), as published by the Free Software
Foundation. See [LICENSE](LICENSE) for details.
