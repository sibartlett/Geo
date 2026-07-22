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
