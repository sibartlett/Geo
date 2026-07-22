# GPS data and file formats

Geo models GPS data and reads/writes the common GPS interchange formats.

## The data model

| Type | Description |
|------|-------------|
| `GpsData` | Top-level container: `Routes`, `Tracks`, `Waypoints`, and `Metadata` |
| `Route` | A planned ordered list of `Waypoints` |
| `Track` | A recorded path, made up of `TrackSegment`s |
| `TrackSegment` | A continuous run of `Waypoints` (fixes) |
| `Waypoint` | A single point — position, optional elevation and timestamp |

`Route`, `Track`, and `TrackSegment` implement `IHasLength`, and the track types
expose helpers such as `GetLength()` (a `Distance`), `GetDuration()` (a
`TimeSpan`), and `GetAverageSpeed()` (a `Speed`):

```csharp
using Geo.Gps;

var track = gpsData.Tracks[0];
var length = track.GetLength();           // Distance
var duration = track.GetDuration();       // TimeSpan
var averageSpeed = track.GetAverageSpeed(); // Speed
```

`Route`, `Track`, and `TrackSegment` also expose `ToLineString()`, and a
`Waypoint` exposes its `Coordinate` and `Point`. These are the bridge from the
GPS types to the [geometry](geometries.md) types, so you can serialize GPS data
with any of the geometry writers (see the recipe below).

## Reading a file

`GpsData.Parse` auto-detects the format from the stream and deserializes it,
returning `null` if no reader recognises the content:

```csharp
using System.IO;
using Geo.Gps;

using var stream = File.OpenRead("activity.gpx");
GpsData data = GpsData.Parse(stream);

foreach (var track in data.Tracks)
foreach (var fix in track.GetAllFixes()) // every Waypoint across the track's segments
{
    // ...
}
```

### Supported formats

| Format | Read | Write |
|--------|:----:|:-----:|
| GPX 1.0 / 1.1 | ✓ | ✓ |
| NMEA | ✓ | |
| IGC | ✓ | |
| Garmin flightplan | ✓ | |
| SkyDemon flightplan | ✓ | |
| PocketFMS flightplan | ✓ | |

Call `GpsData.SupportedGpsFileFormats()` to enumerate the formats known at
runtime. If you want to force a specific reader instead of auto-detecting, each
deserializer implements `IGpsFileDeSerializer` and can be used directly:

```csharp
using Geo.Gps.Serialization;

using var stream = File.OpenRead("flight.igc");
var data = new IgcDeSerializer().DeSerialize(new StreamWrapper(stream));
```

## Converting GPS data to geometries

GPS types live in `Geo.Gps` and geometries live in `Geo.Geometries`, but they are
easy to bridge: `Route`, `Track`, and `TrackSegment` each have a `ToLineString()`
method, and a `Waypoint` exposes its `Coordinate` and `Point`. Once you have a
geometry you can hand it to any geometry writer — [WKT](well-known-text.md),
[WKB](well-known-binary.md), or [GeoJSON](geojson.md).

### Recipe: GPX to GeoJSON

```csharp
using System.IO;
using System.Linq;
using Geo.Geometries;
using Geo.Gps;
using Geo.IO.GeoJson;

using var stream = File.OpenRead("activity.gpx");
GpsData data = GpsData.Parse(stream);

// Turn the first recorded track into a LineString geometry...
LineString line = data.Tracks[0].ToLineString();

// ...and write it as GeoJSON.
string geoJson = new GeoJsonWriter().Write(line);
```

The same `ToLineString()` bridge works for a planned `Route` or a single
`TrackSegment`. To emit the individual waypoints as points instead, build a
`MultiPoint` from their coordinates:

```csharp
using System.Linq;
using Geo.Geometries;
using Geo.IO.GeoJson;

var points = new MultiPoint(data.Waypoints.Select(w => new Point(w.Coordinate)));
string waypointJson = new GeoJsonWriter().Write(points);
```

## Writing GPX

GPX is the only format with a writer. Build a `GpsData` and call `ToGpx`:

```csharp
using Geo.Gps;

var data = new GpsData();
data.Waypoints.Add(new Waypoint(51.5074, -0.1278));

string gpx11 = data.ToGpx();     // GPX 1.1 (default)
string gpx10 = data.ToGpx(1m);   // GPX 1.0
```

A `Waypoint` can be constructed from a latitude/longitude pair, optionally with
elevation and a timestamp:

```csharp
var w1 = new Waypoint(51.5074, -0.1278);
var w2 = new Waypoint(51.5074, -0.1278, elevation: 35);
var w3 = new Waypoint(51.5074, -0.1278, 35, DateTime.UtcNow);
```
