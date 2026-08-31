# Changelog

All notable changes to [Geo](https://nuget.org/packages/Geo) are documented in
this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [3.0.0] — 2026-08-31

A NativeAOT release — and, because of what that required, a GPX release.

`XmlSerializer` cannot be published natively: it generates and compiles a
serialization assembly at runtime by reflecting over attributed types. Any
application that published natively and touched a `Geo` GPS serializer failed at
runtime, so the library could not be used from one at all. GPX 1.0/1.1 and the
Garmin, PocketFMS and SkyDemon flightplan formats are now read and written with
`System.Xml.Linq`, which needs no runtime code generation, and the ~70 model
classes that existed only to be bound by `XmlSerializer` are gone with it.

The unit conversions had the same problem for the same reason: `UnitMetadata` read
each unit's symbol and factor off the enum members' `[Unit]` attributes through
`Enum.GetValues(Type)` and `Type.GetField`. The definitions are now a plain table,
resolved with no reflection.

Rewriting the GPX serializers meant reading the schemas properly, which turned up
how much the old ones had been dropping. Extensions, links, and the file-level
`<time>` and `<bounds>` are now carried; `<gpx>` gets the `creator` attribute the
schema requires of it; and every element is written in the order its schema
sequences them, which the old output did not do. Written GPX is now validated
against the official schemas as part of the test suite, so it stays that way.

The library multi-targets `netstandard2.0` and `net8.0`. The modern target sets
`IsAotCompatible`, so the trimming and AOT analysers gate the build, and a new
`Geo.AotSmokeTest` project publishes natively and exercises every serializer as
part of CI.

Nothing about that narrows where the package runs: **.NET Standard 2.0** is still
a target, and Geo still has no dependencies. The `net8.0` asset exists so the
analysers have something to analyse.

### Breaking changes

The first two are what ordinary callers touch. The last two affect only code
that referenced the serialization models directly or subclassed the XML base
classes to add a format.

- **`GpsData.ToGpx` takes a `GpxVersion` rather than a `decimal`.** The old
  parameter recognised one value and silently wrote GPX 1.1 for everything else —
  so `ToGpx(1)` gave 1.0, but `ToGpx(1.0m)` and `ToGpx(99)` both gave 1.1.

  ```csharp
  // before
  data.ToGpx(1m);              // GPX 1.0
  data.ToGpx(1.1m);            // GPX 1.1, and so did any other value

  // after
  data.ToGpx(GpxVersion.Gpx10);
  data.ToGpx(GpxVersion.Gpx11);   // also the default, so ToGpx() is unchanged
  ```

  A version the writer does not recognise now throws `ArgumentOutOfRangeException`
  instead of quietly producing the other one. `ToGpx()` with no argument still
  writes GPX 1.1.

- **The `link` metadata attribute has been replaced by `Links`.**
  `data.Metadata.Attribute(x => x.Link)` no longer compiles; use `data.Links`
  instead. The attribute could hold only a single address and dropped the link's
  text, which lost 82 of the 88 links in the reference corpus — most of them on
  waypoints, where the attribute had no equivalent at all.

  ```csharp
  // before
  data.Metadata.Attribute(x => x.Link, "https://example.com");
  var href = data.Metadata.Attribute(x => x.Link);

  // after
  data.Links.Add(new GpsLink("https://example.com"));
  var href = data.Links.FirstOrDefault()?.Href;
  ```

  `Author.Link` is unchanged. GPX allows a person only one link, so the single
  string attribute still expresses it.

- **The XML serialization models are gone.** Every type under
  `Geo.Gps.Serialization.Xml.Gpx`, `.Garmin`, `.PocketFms` and `.SkyDemon`
  (`GpxFile`, `GpxWaypoint`, `PocketFmsMeta`, `SkyDemonRoute` and the rest) existed
  only to be bound by `XmlSerializer` and has been removed. The documents are now
  read straight into `GpsData`.

- **`GpsXmlDeSerializer<T>` and `GpsXmlSerializer<T>` are no longer generic.** They
  are now `GpsXmlDeSerializer` and `GpsXmlSerializer`; the abstract members a
  subclass implements take an `XElement` and return an `XDocument` rather than
  taking and returning the removed model types. `IGpsFileDeSerializer` and
  `IGpsFileSerializer` are unchanged, so code that consumes serializers through the
  interfaces — including `GpsData.Parse` and `GpsData.ToGpx` — is unaffected.

### Added

- **Links are read and written, as a typed model.** `GpsData`, `Waypoint`, `Route`
  and `Track` each expose a `Links` collection of the new `GpsLink` (`Href`, `Text`,
  `Type`):

  ```csharp
  var waypoint = data.Waypoints[0];
  waypoint.Links.Add(new GpsLink("https://example.com", "More about here", "text/html"));
  ```

  GPX 1.1 allows any number of `<link>` elements on each of those four, with a text
  and a media type. GPX 1.0 has a single `<url>`/`<urlname>` pair in the same
  places, so a document written as 1.0 keeps the first link and drops every media
  type — there is nowhere in that version to put them. Reading 1.0 fills the same
  collection, so links cross between the versions.

  This replaces the `link` metadata attribute, which held one address and dropped
  its text — see **Breaking changes**. It also picks up `<urlname>` and the
  per-element `<url>`s, neither of which was read or written before.

- **The file-level `<time>` and `<bounds>` are no longer dropped.** 56 of the 65
  reference files carry a `<time>` and 55 a `<bounds>`; both went unread and
  unwritten. They are handled differently, because they are different kinds of
  thing:

  - `GpsMetadata.TimeUtc` (`DateTime?`) holds when the file says it was created —
    something only the file can tell you, so it is kept and written back. A
    property rather than one of the keyed metadata attributes because those are
    strings and GPX declares the element `xsd:dateTime`; a value kept as text
    could be written back in a form no other reader would accept.
    `Waypoint.TimeUtc` already carried a GPX time this way.
  - `<bounds>` is **computed at write time, not stored.** GPX defines it as the
    extent of the coordinates in the file, so keeping the file's copy would mean
    writing back an extent that stopped being true the moment a caller added a
    waypoint.

- **`GetBounds()` on `GpsData`, `Track`, `TrackSegment` and `Route`**, returning
  the `Envelope` covering their coordinates, or `null` when they hold none. There
  was previously no way to ask any of them for its extent.

  Two consequences of computing rather than storing. Of the 55 reference files
  with a `<bounds>`, 53 match what Geo computes and 2 do not — in both the file's
  own bounds disagrees with its own coordinates, and the written output now
  corrects it. And because `<bounds>` lives inside `<metadata>` in GPX 1.1, a
  document with data but no metadata now gains a `<metadata>` element holding just
  the bounds, where the element used to be omitted.

- **GPX extensions are read and written** ([#64]). `GpsData`, `Track`,
  `TrackSegment`, `Route` and `Waypoint` each expose an `Extensions` collection
  holding the foreign content of their GPX element as `XElement`s. Anything read is
  written back, so extension data is no longer silently dropped on a round-trip,
  and a caller can query it with LINQ to XML:

  ```csharp
  XNamespace style = "http://www.topografix.com/GPX/gpx_style/0/2";

  var colour = data.Tracks[0]
      .Extensions.FirstOrDefault(x => x.Name == style + "line")
      ?.Element(style + "color")?.Value;
  ```

  The content is handed over as XML rather than modelled because `<extensions>` is
  deliberately open — no fixed set of properties could keep up with what Garmin,
  Gaia GPS, the Topografix `gpx_style` schema and the rest put in there. Both GPX
  versions are supported, each written in its own shape: 1.1 wraps the content in
  an `<extensions>` element, while 1.0 has no such element and carries it inline.

  Three limits worth knowing. A `TrackSegment`'s extensions are 1.1-only — the GPX
  1.0 schema ends `<trkseg>` with `<trkpt>` and admits no foreign element after
  it — so writing 1.0 drops them. Content a 1.1 document holds in
  `<metadata><extensions>` is read into `GpsData.Extensions` and written back at
  the `<gpx>` level, which is where 1.0 would carry it; reading that output again
  gives the same result. And an `<extensions>` element in a GPX 1.0 document — not
  something that version has, but writers emit one anyway — is read, except for
  children left in the GPX namespace: 1.0 carries extensions inline, so an
  unprefixed `<ele>` moved out of `<extensions>` would be indistinguishable from a
  real elevation.

### Fixed

- **Written GPX always carries its `creator` attribute.** Both schemas declare it
  `use="required"`, but it was written only when the metadata said what produced
  the file — so every document Geo built from scratch was invalid against the
  schema its own root element announces. A `creator` read from a file is still
  preserved rather than replaced; only the empty case now falls back, to `Geo`.

  GPX output is now validated against the bundled `reference/schemas/gpx10.xsd`
  and `gpx11.xsd` as part of the test suite — what every reference file writes
  back in both versions, plus documents built by hand. That is what turned this
  up, and it is a standing gate on the class of bug where Geo's reader and writer
  agree with each other but not with GPX.

- A PocketFMS flightplan with no `<LIB>` leg, or one whose points carry no
  coordinates, is reported by returning `null` like any other document the
  deserializer cannot read. It used to index the first leg and dereference an
  absent `<META>` regardless, raising `IndexOutOfRangeException` or
  `NullReferenceException` out of `GpsData.Parse`.
- A Garmin flightplan whose waypoint table repeats an identifier no longer throws;
  the first entry wins. A route point naming a waypoint that is not in the table is
  skipped rather than raising.

### Changed

- GPX output now orders each element's children as the GPX schemas sequence them.
  `XmlSerializer` emitted inherited members first, which put `<link>` and `<fix>`
  after the rest of a `<wpt>` and made the output invalid against the XSD. Parsing
  is unaffected — element order was never significant on read.

- The `[Unit(...)]` annotations no longer appear on `AreaUnit`, `DistanceUnit` and
  `SpeedUnit`, and `UnitAttribute` is gone with them. Both were `internal`, so no
  consumer could reference either; the symbols and conversion factors they carried
  are unchanged.

- `Spatial2DComparer<T>.Equals` and `Spatial3DComparer<T>.Equals` are now declared
  `Equals(T? x, T? y)`. The `net8.0` reference assemblies annotate
  `IEqualityComparer<T>` where netstandard2.0's do not, so the annotation is
  required to implement the interface without a warning. Both already forwarded to
  a null-tolerant comparison, so behaviour is the same — but code compiled with
  nullable reference types may see warnings shift.

## [2.0.0] — 2026-08-04

The first major release since 1.0.0. It is a large correctness release: much of
the library gained real test coverage for the first time (line coverage went from
roughly 30% to over 94%), and that turned up a long list of bugs — several of
which silently returned wrong positions, areas or magnetic fields rather than
failing. Those fixes, together with an alignment to OGC/NTS conventions and a
project-wide switch to nullable reference types, are why this is a major version.

The library still targets **.NET Standard 2.0** and has no new dependencies.

### Breaking changes

#### Measure

- **`Area` is now expressed in `AreaUnit`, not `DistanceUnit`.** Its unit
  constructor, `Unit` property, `Value`, `ToString(unit)` and `ConvertTo` all take
  or return `AreaUnit`, and `ConvertTo` now returns an `Area` rather than a
  `Distance`. Conversion factors are squared, as they must be for an area:
  `new Area(2, AreaUnit.Km).SiValue` is now `2_000_000`, where
  `new Area(2, DistanceUnit.Km).SiValue` used to be `2_000`. The square-metre
  `SiValue`, arithmetic and comparison operators are unchanged, and the geodetic
  calculators only ever construct `Area` from square metres — so computed geometry
  areas are unaffected by this item. ([#110])
- `AreaUnit`'s symbols (`m²`, `km²`, …) had been mangled to the Unicode
  replacement character; they are repaired and now surface through `ToString`.
  The statute-mile factor is the exact 1609.344 m. ([#110], [#119])

#### Geometry types

- **`Point.Coordinate` is now get-only.** A point's equality and hash code derive
  from it, so a settable coordinate let a point rewrite its own key while sitting
  in a dictionary. Source- and binary-breaking. ([#124])
- **Geometry `Empty` members are now static properties returning a fresh instance**
  instead of shared `static readonly` fields, on `Point`, `LineString`,
  `LinearRing`, `Polygon`, `Triangle`, `Circle`, `MultiPoint`, `MultiLineString`,
  `MultiPolygon` and `GeometryCollection`. Binary-breaking, and `Empty` no longer
  has reference identity. ([#107])
- **Ring and triangle validation now follows OGC Simple Features / NTS.**
  `LinearRing` requires zero or at least four coordinates, so a closed three-point
  sequence such as `[A, B, A]` is rejected. `Triangle` rejects any closed ring that
  is not exactly four coordinates (the guard used `&&` where it needed `||`, so
  five-point rings were accepted). ([#105])
- `LineString.IsClosed` and `CoordinateSequence.IsClosed` now share one definition
  — non-empty, more than one point, coincident endpoints. They previously
  disagreed, and a single-coordinate `LineString` reported `IsClosed == true`.
  ([#105])
- `Polygon.Is3D` / `IsMeasured` now consider holes, not just the shell. ([#118])

#### Equality and hashing

- **Hash codes no longer depend on `GeoContext.Current.EqualityOptions`.** The
  parameterless `GetHashCode()` hashes the position alone; elevation and measure
  are left out, because whether they count towards equality is exactly what the
  ambient options vary. Previously, changing those options rehashed every geometry
  and a live `HashSet`/`Dictionary` stopped finding entries it already held. Pole
  and anti-meridian longitudes are now collapsed unconditionally. Coordinates
  differing only in elevation or measure now share a hash bucket — a collision, not
  an equality change; use the `GetHashCode(SpatialEqualityOptions)` overload (as
  `Distinct3D` and `Spatial3DComparer` do) where they should be spread. ([#125])
- **Cross-dimension coordinate equality now honours the options.** Each `Equals`
  demanded the other side be its own concrete type, so `Equals2D` was false for a
  `Coordinate` against a `CoordinateZ` at the same position — and `Distinct2D` and
  `Spatial2DComparer`, whose purpose is to collapse exactly that pair, kept both.
  ([#119])
- **`LineSegment` and `GeodeticLine` equality is now symmetric.** `segment.Equals(line)`
  returned `true` while `line.Equals(segment)` returned `false`, with different hash
  codes on each side; both now match on the runtime type. ([#126])

#### Serialization contracts

- **Malformed input is now reported as `SerializationException`** across the
  readers, instead of escaping as `ArgumentOutOfRangeException`,
  `ArgumentException` or `FormatException` naming a parameter the caller never
  supplied. This covers `WktReader`, `WkbReader`, `GeoJsonReader` and
  `GooglePolylineEncoder.Decode`, and applies to out-of-range ordinates,
  non-closing rings, unparseable numbers and structurally complete but impossible
  bytes. The original exception is kept as `InnerException` where one exists.
  ([#117], [#118], [#121])
- `GeoJsonReader.TryRead` and the `TryParse*` methods now return `false` on the
  malformed input that previously threw out of them. ([#116], [#121])
- **WKB: an empty `Point` is now written as a 2D point with `NaN` ordinates**,
  matching OGC/NTS/GEOS/PostGIS, instead of zero bytes (which is not a valid WKB
  record and read back as `null`). Empty points are no longer dropped from
  `MultiPoint` and `GeometryCollection`, so a collection keeps its cardinality
  across a round trip. ([#106])
- `WkbReader.Read(byte[])` now rejects trailing bytes after the geometry. The
  stream overloads deliberately do not, since a stream may carry more than the
  geometry. ([#118])
- **GeoJSON: a feature whose geometry cannot be read now fails the parse** rather
  than producing a feature with a null geometry, indistinguishable from a
  genuinely unlocated one. ([#119])
- Nested objects and arrays in GeoJSON feature properties are now returned as
  their underlying JSON values. They were previously recursed with the whole
  `KeyValuePair` rather than its value, so leaves came back wrapped in boxed
  key/value pairs. ([#99])
- `IGpsFileSerializer` gains `SerializeAsync`; external implementers must add it.
  ([#91])
- Both GPX serializers report their file extension as `gpx`, not `gps`.
  `SupportedGpsFileFormats` is public API used to build file filters. ([#115])

#### Coordinate parsing

- **Minutes and seconds are now unsigned, and a separator is required.** A sign
  belongs to the degrees field or the hemisphere letter, and no notation writes
  `51° -30′` to mean anything but 51°30′. This is the one place a
  previously-parsing input changes value: `"51 -30, 0"` is now `51.5` where it was
  `50.5`. Degrees keep their sign. Requiring a separator also means inputs like
  `"1+2, 3"` and `"1.2.3, 4"` now fail instead of returning a position, and removes
  an exponential-backtracking denial of service — 40 digits took over 5 seconds to
  reject and now takes under a millisecond. ([#123])
- A hemisphere letter must now agree with the axis it sits on. Only `S` and `W`
  were ever tested, so a letter naming the other axis was silently dropped and
  `"0.12W, 51.5N"` parsed with both ordinates positive. ([#123])
- `Coordinate.TryParse` returns `false` instead of throwing, for `null` input and
  for out-of-range ordinates. `Parse` still rejects `null` up front. ([#70], [#116])

#### Geodesy and geomagnetism

- **`SpheroidCalculator` now honours the `Spheroid` it was constructed with** for
  every area and for circle and envelope lengths. Five members previously delegated
  to a hardcoded sphere of the WGS-84 mean radius, so the datum made no difference
  to any of them. `Envelope.GetArea()`, `Polygon.GetArea()`, `Envelope.GetLength()`
  and the corresponding `IGeodeticCalculator` members move by **+0.44% at the
  equator to −0.87% at high latitude** (about half that for lengths). Corrections in
  every case, but different numbers. ([#120])
- **`SpheroidCalculator.CalculateOrthodromicLine(point, heading, distance).Bearing21`
  now returns degrees.** It was returning radians reduced mod 2π, which lands in
  [0, 6.28) and so read as a plausible bearing — nothing threw, callers just got a
  wrong answer. ([#120])
- `Circle.GetBounds()` returns a different, larger box: it had been measuring in
  nautical-mile-per-arcminute units, implying a sphere of 6 366 707 m while
  `ToPolygon()` projects onto the ambient spheroid, so the box came out ~0.5% short
  and excluded the geometry it bounds. ([#120])
- `SphereCalculator.CalculateArea(CoordinateSequence)` returns an unsigned area.
  It had returned a signed area whose sign was inverted relative to the library's
  own winding convention, so a standards-wound polygon (counter-clockwise shell,
  clockwise holes) had its holes *added* rather than subtracted. ([#102])
- Geomagnetic field values change everywhere. See *Fixed* below — the WMM2015,
  reference-radius, WMM2020 coefficient and IGRF secular-variation corrections all
  move computed values. ([#89], [#129], `ad9d553`)

#### Nullability

- **Nullable reference types are enabled project-wide**, and the public API is
  annotated to reflect the nullability the code already relied on. Consumers
  compiling with NRT on will see new warnings where the library returns null.
  Annotations are metadata-only and binary-compatible; the signatures that changed
  meaning include `GpsData.Parse` → `GpsData?`, `IGeometry.GetBounds()` →
  `Envelope?`, `IGeodeticCalculator.CalculateOrthodromicLine(p1, p2)` and
  `CalculateLoxodromicLine` → `GeodeticLine?`, `IGpsFileDeSerializer.DeSerialize` →
  `GpsData?`, and `GeomagnetismCalculator.TryCalculate` → `out GeomagnetismResult?`.
  ([#93], [#94], [#95])
- **`GetBounds()` on an empty `Point`, `Polygon` or `Circle` returns `null`**
  instead of throwing `NullReferenceException`, matching what `LineString`,
  `GeometryCollection` and `CoordinateSequence` already did. ([#93])

#### Other

- `GpsFeaturesExtensions.Contains` now requires **all** requested features rather
  than any overlapping one, matching its name and `Enum.HasFlag` semantics. Every
  existing caller passed a single flag, where the two are identical;
  `GpsData.SupportedGpsFileFormats(GpsFeatures.TracksAndWaypoints)` had wrongly
  listed IGC, which stores tracks only. ([#109])

### Added

- **`GeoFormat`** — a format-detecting entry point for an unknown geospatial
  string, with the new `GeoStringFormat` enum. `Detect(string)`, `Parse(string)`
  and `TryParse(string, out object, out GeoStringFormat)` cover coordinate strings,
  WKT and GeoJSON, delegating to the existing parsers so detection always agrees
  with parsing. (issue [#15], [#92])
- **Async deserialization** — `GpsData.ParseAsync(Stream, CancellationToken)`,
  plus `ReadAsync` on `WktReader`, `WkbReader` and `GeoJsonReader`, `WriteAsync` on
  `WkbWriter`, and `SerializeAsync` on the GPS serializers. (issue [#48], [#91])
- **Coordinate ordering and winding helpers** in `Geo.Linq` —
  `OrderClockwise()`, `OrderCounterClockwise()` and `GetWindingOrder()`, with a new
  `WindingOrder` enum, for turning an unordered set of coordinates into a polygon
  shell. The maths is spherical: ordering sorts by azimuth in the tangent plane at
  the centroid direction, and winding comes from the signed spherical area, so
  rings crossing the antimeridian and rings enclosing a pole are handled correctly.
  (issue [#54], [#96], [#100])
- **`Spheroid.AuthalicRadius`** — the radius of the sphere with the same surface
  area (6 371 007.181 m for WGS-84). Substituting authalic latitudes turns the
  spherical area formulas into exact spheroidal ones. ([#120])
- **`SphereCalculator` is complete.** Six members threw `NotImplementedException`,
  including the inverse orthodromic line — so it could not measure the distance
  between two points, and setting it as the ambient calculator broke
  `LineString.GetLength()` and `Circle.ToPolygon()`. All six now have closed forms.
  ([#120])
- **GPX files with a missing `xmlns` are now parsed**, via a
  `NamespaceCoercingXmlReader` that reports the expected GPX namespace for elements
  in no namespace. Some real-world exports omit it, and such files previously
  returned `null` from `GpsData.Parse`. Documents in a different, non-empty
  namespace are deliberately left alone. (issue [#55], [#90])
- **IGRF coverage extends from 1900–2015 to 1900–2030.** The epoch count is now
  derived from the coefficient data rather than hardcoded, so the IGRF-14 tables'
  values through 2025 and their trailing secular-variation column are used.
  (`43afb0b`)
- Coordinate parsing accepts two more notations: a **leading hemisphere letter**
  (`N51 30.0, W000 07.2`), which aviation and marine sources overwhelmingly use,
  and **hyphen-separated DMS** (`40-26-46N, 079-56-55W`), the FAA/NGS form.
  ([#123])
- The WKT tokenizer accepts a `+` sign, so positive exponents such as `1.5E+21` —
  which .NET's own `double.ToString` emits for large elevations, and which
  `WktWriter` therefore produced — read back, as does a leading `+` on an ordinate.
  ([#111])
- A documentation suite under [`docs/`](docs/), covering coordinates, geometries,
  envelopes, geodesy, geomagnetism, GPS, measures, `GeoContext`, WKT, WKB, GeoJSON
  and parsing, plus a [Geo vs. NetTopologySuite](docs/geo-vs-nts.md) comparison.
  Every C# example is compiled against the real API by a test, so they cannot drift.
  ([#85], [#114])
- The NuGet package now carries the README and the project URL ([#84]), and a
  logo and package icon (`856aa56`).

### Fixed

#### Coordinates and parsing

- `Coordinate.TryParse` lost the sign of a degrees field written as `-0`: the
  direction was decided by testing the parsed value, and negative zero is not less
  than zero, so minutes and seconds were added rather than subtracted. Every DMS/DM
  coordinate in the (−1, 0) degree band — where most of western Europe's longitudes
  sit — landed on the wrong side of the meridian, up to 111 km out. ([#122])
- `Coordinate.TryParse` read degrees with the invariant culture but minutes and
  seconds with the current culture, so under a comma-decimal culture
  `"12 34.56'N"` parsed as 12 degrees 3456 minutes. `Coordinate.ToString` had the
  mirror problem, emitting `"51,5, -0,12"` which `Parse` could not read back.
  ([#115])
- The range guard in `TryParse` used `||` where it needed `&&`, so only one
  ordinate had to be in range before construction — and the constructor then threw.
  ([#70])
- The seconds-marker character class contained a stray backslash, written as if
  `\"` escaped a quote inside a verbatim string, so `\` matched as a seconds
  marker. ([#123])

#### Geometries and equality

- The spatial `Equals` overload on all four coordinate types mis-parenthesised the
  pole check as `(options.PoleCoordiantesAreEqual && Latitude.Equals(90d)) || Latitude.Equals(-90d)`,
  so any two coordinates at the **south** pole compared equal regardless of
  longitude, even with the option off — and contradicted `GetHashCode`, which had
  the correct precedence. ([#67])
- `CoordinateM.GetHashCode` gated the measure on `options.UseElevation` instead of
  `UseM`. A `CoordinateM` has no elevation, so under the default options two
  coordinates that compared equal produced different hash codes. ([#75])
- `Polygon.Equals` threw `NullReferenceException` when an empty polygon was
  compared to a non-empty one, and was asymmetric — the reverse direction returned
  `false`. It only returned early when *both* sides were empty, then dereferenced a
  null `Shell`. Affects `Triangle` and the `==`/`!=` operators too. ([#113])
- `Polygon.GetArea()` dereferenced a null `Shell`, so `Polygon.Empty.GetArea()`,
  `Triangle.Empty.GetArea()` and `MultiPolygon.GetArea()` over any collection
  holding an empty member threw. An empty polygon now returns zero area. ([#115])
- `Circle.ToPolygon()` dereferenced a null centre for an empty circle, which made
  the WKT, WKB and GeoJSON writers throw when configured to convert circles to
  regular polygons. ([#116])
- `Envelope.Intersects` tested whether a corner of one envelope fell strictly
  inside the other, which misses cross-shaped overlaps — a bar crossing through a
  box — and reported identical or edge-sharing envelopes as non-intersecting. It
  now uses the standard axis-aligned interval-overlap test. ([#104])

#### Geodesy

- `SphereCalculator.CalculateArea(Envelope)` used `Math.Cos` where the spherical
  zone formula needs `Math.Sin`, so the whole sphere returned **0** instead of
  4πR², and an ordinary envelope returned a **negative** area.
  `CalculateLength(Envelope)` had broken operator precedence
  (`180 / MaxLat - MinLat`) and derived each parallel's radius as `R·sin(lat)`
  instead of `R·cos(lat)`. ([#66])
- `Circle.GetBounds()` returned an envelope no coordinate could sit inside near a
  pole: a 100 km circle at 89.5°N reported a maximum latitude of 90.4, and at
  89.9°N a longitude span of ±515° (±1.5e16 at the pole itself). It now clamps to
  the pole, spans every longitude when the circle reaches one, handles
  anti-meridian wrap, and takes the east-west extent from the tangent meridians
  rather than a small-angle approximation. ([#119], [#120])
- `Circle.GetBounds()` converted the radius into degrees of longitude by
  *multiplying* by cos(latitude) instead of dividing, making the box narrower away
  from the equator when it must be wider. At 60°N a 111 km circle is about 2° tall
  and ~4° wide; it was reported as ~1° wide. ([#112])
- `JulianDate.DateToJD` computed `(second + millisecond * 1000) / 86400`, scaling
  milliseconds up by 1000 instead of converting them to seconds. ([#73])

#### Geomagnetism

- **WMM2020's `h[8,7]` coefficient was `+8.0` where the World Magnetic Model has
  `-6.9`** — the largest disagreement with IGRF-13 anywhere in the nine shared
  epochs. Correcting it more than halves the worst-case total-intensity
  disagreement between the two models for 2020–2024. A new `GeomagneticModelTests`
  checks the coefficient tables themselves — holding both model families to
  structural properties rather than to pinned values — which is what found it; the
  pinned baselines could not, since they are generated from the tables. ([#129])
- **The library disagreed with NOAA's calculator by ~35 nT / ~0.05° for 2015–2020.**
  Two causes: NOAA replaced the original WMM2015 with WMM2015v2 in 2018, and the
  spherical-harmonic expansion used the WGS-84 mean radius where WMM/IGRF define a
  fixed 6371.2 km reference radius. Both fixed; the spheroid is now used only for
  the geodetic-to-geocentric conversion. (issue [#38], [#89])
- The IGRF factory built its terminal epoch's secular-variation coefficients from
  the *absolute* field values at the next epoch rather than the rate of change, so
  the whole field was scaled by (1 + yearfrac) — inflating intensities by up to ~5×
  late in the epoch while leaving declination and inclination nearly unchanged.
  (`ad9d553`)
- `GeomagnetismResult` discarded the entire field, including the vertical
  component, whenever *x* or *y* was zero — so a field pointing straight down
  reported no field. `Math.Atan2` is defined for a zero argument and needs no such
  guard. ([#119])

#### Serialization

- `WkbBinaryReader` used `BinaryReader.PeekChar` to test whether the stream held
  data. `PeekChar` returns −1 for any non-seekable stream, so `WkbReader.Read(Stream)`
  silently returned `null` for a perfectly valid geometry arriving over a network,
  pipe or compression stream. `ReadBytes` also now loops until the requested count
  is satisfied. ([#115])
- Truncated WKB surfaced as `ArgumentOutOfRangeException` from `BitConverter`
  rather than the intended `SerializationException`, because `BinaryReader.ReadBytes`
  returns a short array instead of throwing. ([#74])
- **The WKB and WKT writers could produce output the library could not read back.**
  Both declare dimensions once for a whole geometry but wrote ordinates per
  coordinate, with nothing keeping the two in step — so a sequence mixing 2D and 3D
  coordinates, or a polygon whose elevations live only in a hole, wrote a malformed
  record with no error raised. This is reachable from ordinary data: `<ele>` is
  optional per trackpoint in GPX, so a device losing its altitude fix mid-track
  produces exactly such a sequence. Dimensions are now carried as one value from the
  declaration through to every coordinate, and missing ordinates are padded (`NaN`
  in WKB, `NullOrdinate` in WKT). Uniform geometries are byte-for-byte unchanged.
  ([#118])
- WKB coordinate counts are read off the input and were used to size a list, so a
  four-byte header could demand an arbitrary allocation before a single coordinate
  was read; a count above `int.MaxValue` turned negative and either threw or
  silently returned an empty geometry. Counts are now unsigned and the list grows
  into what actually arrives. ([#118])
- `WktReader` read `TRIANGLE` as a plain `Polygon`, so a triangle could not survive
  a WKT round trip and its ring went unvalidated. `WkbReader` already read it as a
  `Triangle`. ([#121])
- `GeoJsonWriter` threw `NullReferenceException` writing any empty `Point` or
  `Polygon`, and so for any `MultiPoint`, `MultiPolygon`, `Feature` or
  `FeatureCollection` containing one. Empties are now written as an empty
  coordinates array — the NTS/GEOS/PostGIS convention — and read back, so they round
  trip. ([#116])
- `GeoJsonWriter.Write(object)` threw `NullReferenceException` for a null argument
  while building the "not supported by GeoJSON" message out of its type. ([#122])
- **`GooglePolylineEncoder.Decode` accepted malformed polylines and returned
  plausible but wrong coordinates.** End-of-string and an out-of-range character
  were indistinguishable from a legitimate final chunk, and the `& 0x1f` mask folded
  the junk into the result as data bits. A trailing newline — routine when a
  polyline comes from a file or a multi-line API response — fabricated an extra
  coordinate; input cut mid-number gave −0.10528 for −120.2. Truncation, invalid
  characters and overflow are now reported as `SerializationException` naming the
  offset. Valid polylines decode exactly as before. ([#117])
- `GooglePolylineEncoder.Encode` cast the scaled ordinate to `int`, truncating
  toward zero where the algorithm rounds to nearest, so output disagreed with
  Google's reference encoder by one unit in the last place. ([#108])

#### GPS formats

- **IGC and NMEA tracks running past midnight UTC went twenty-four hours
  backwards.** Fixes in both formats carry a time of day and nothing else, and both
  deserializers stamped every fix onto the same day, leaving `GetDuration` and
  `GetAverageSpeed` negative. A new rollover clock advances the date whenever the
  clock goes backwards. ([#122])
- **One unreadable NMEA sentence discarded the entire log.** `ConvertOrd` split each
  ordinate at the format's fixed degrees width, which the matching pattern does not
  enforce, so a short field ran off the end of the string or left the minutes empty
  for `double.Parse`. An NMEA log is a stream of sentences, not a single document,
  and any long recording is likely to hold a corrupt one — so a single bad line
  returned nothing at all. Such a sentence is now skipped, which is how the parser
  has always treated a line it could not match. ([#128])
- **SkyDemon coordinates could be misread as a success.** The seconds pattern used a
  bare `.` where it meant a literal decimal point, so a file writing its separator as
  a comma matched and `"07,00"` reached `double.Parse` as **seven hundred** — putting
  the position ~21 km north and ~92 km west with nothing reported. Every other
  malformed coordinate threw (`FormatException`, `IndexOutOfRangeException`,
  `ArgumentOutOfRangeException` naming `latitude`) instead of returning `null` as the
  deserializer does for a document it cannot parse. Four inputs that were never
  malformed — seconds with one, three or no decimal places, extra whitespace between
  ordinates, and a route consisting only of its starting point — now parse. ([#127])
- Both NMEA and SkyDemon judged each ordinate against `"N or E"`, which worked only
  because a latitude can never carry an `E`, and silently made a latitude southern
  whenever the match had failed. Each ordinate is now judged by its own hemisphere
  letter, and both parse with `NumberStyles.Float` so a thousands separator cannot
  be mistaken for part of a number. ([#127], [#128])
- `IgcDeSerializer` resolved a file's two-digit year against a pivot from
  `DateTime.UtcNow.ToString("yy")`, which reads the year through the current
  culture's calendar. Under a non-Gregorian default every fix landed decades out — a
  2026 flight dated 1983 under `th-TH`, 2004 under `ar-SA`. ([#121])
- Both GPX serializers dereferenced `rteType.rtept` without a null check. `<rtept>`
  is optional in the GPX schema, so a `<rte>` carrying only metadata is valid and
  faulted the parser. ([#116])
- Serializing GPX 1.1 metadata whose author email had no `@` threw
  `IndexOutOfRangeException` out of the middle of the document. The element is now
  left out when the address cannot be split. ([#119])
- `TrackSegment.GetDuration` threw `NullReferenceException` on an empty segment,
  since `GetFirstWaypoint`/`GetLastWaypoint` return null for one. (`31b4606`)
- `StreamWrapper`'s constructor buffered a non-seekable source but left the buffer
  positioned at the end of the copy, so the wrapper read as empty until something
  happened to seek it — unlike a wrapper over a seekable stream, and unlike
  `CreateAsync`, which has always rewound. ([#121])

#### Measure

- `Distance`, `Area` and `Speed` derived `CompareTo` from "neither equal nor less
  than, therefore greater than", which does not hold for a `NaN` measure: each side
  reported itself the greater, and sorting such a set shuffled the surrounding
  values out of order. All three now delegate to `Double.CompareTo`. ([#122])
- Unit conversion factors were rounded decimals rather than the exact ratios that
  define the units, so 10 m/s converted to 35.99997 kph instead of 36, and every
  mile came out 4 mm short of the international 1609.344 m. ([#119])

### Changed

- **The vendored `SimpleJson.cs` (2,516 lines) is replaced** by a compact internal
  JSON reader/writer scoped to what GeoJSON actually needs. Most of SimpleJson —
  POCO reflection, `dynamic`, `DataContract`, generic deserialization — was unused.
  Serialized GeoJSON is byte-for-byte identical. The `SIMPLE_JSON_TYPEINFO` compile
  constant is gone. All the affected types were internal, so there is no public API
  change. ([#98])
- `Spatial2DComparer` and `Spatial3DComparer` no longer rebuild their options from
  the ambient settings on every hash, which removed all per-hash allocation
  (4,687 KB → 0 KB over 200,000 hashes) and roughly halved the time. ([#125])
- Build and test tooling moved to the .NET 10 SDK. The library's target framework
  is unchanged at netstandard2.0, and its package dependencies are unchanged.

## Earlier releases

Releases before 2.0.0 predate this changelog. See the
[commit history](https://github.com/sibartlett/Geo/commits/master) and the
[NuGet version list](https://www.nuget.org/packages/Geo#versions-body-tab) for
details.

| Version | Released |
|---------|------------|
| 1.2.0   | 2025-06-17 |
| 1.1.1   | 2025-01-03 |
| 1.1.0   | 2025-01-01 |
| 1.0.2   | 2024-10-02 |
| 1.0.1   | 2023-03-04 |
| 1.0.0   | 2020-05-22 |
| 0.20.0  | 2020-02-14 |
| 0.14.1  | 2016-04-09 |
| 0.14.0  | 2015-12-29 |
| 0.13.0  | 2015-08-18 |
| 0.12.2  | 2015-01-10 |

[#15]: https://github.com/sibartlett/Geo/issues/15
[#38]: https://github.com/sibartlett/Geo/issues/38
[#48]: https://github.com/sibartlett/Geo/issues/48
[#54]: https://github.com/sibartlett/Geo/issues/54
[#55]: https://github.com/sibartlett/Geo/issues/55
[#64]: https://github.com/sibartlett/Geo/issues/64
[#66]: https://github.com/sibartlett/Geo/pull/66
[#67]: https://github.com/sibartlett/Geo/pull/67
[#70]: https://github.com/sibartlett/Geo/pull/70
[#73]: https://github.com/sibartlett/Geo/pull/73
[#74]: https://github.com/sibartlett/Geo/pull/74
[#75]: https://github.com/sibartlett/Geo/pull/75
[#84]: https://github.com/sibartlett/Geo/pull/84
[#85]: https://github.com/sibartlett/Geo/pull/85
[#89]: https://github.com/sibartlett/Geo/pull/89
[#90]: https://github.com/sibartlett/Geo/pull/90
[#91]: https://github.com/sibartlett/Geo/pull/91
[#92]: https://github.com/sibartlett/Geo/pull/92
[#93]: https://github.com/sibartlett/Geo/pull/93
[#94]: https://github.com/sibartlett/Geo/pull/94
[#95]: https://github.com/sibartlett/Geo/pull/95
[#96]: https://github.com/sibartlett/Geo/pull/96
[#98]: https://github.com/sibartlett/Geo/pull/98
[#99]: https://github.com/sibartlett/Geo/pull/99
[#100]: https://github.com/sibartlett/Geo/pull/100
[#102]: https://github.com/sibartlett/Geo/pull/102
[#104]: https://github.com/sibartlett/Geo/pull/104
[#105]: https://github.com/sibartlett/Geo/pull/105
[#106]: https://github.com/sibartlett/Geo/pull/106
[#107]: https://github.com/sibartlett/Geo/pull/107
[#108]: https://github.com/sibartlett/Geo/pull/108
[#109]: https://github.com/sibartlett/Geo/pull/109
[#110]: https://github.com/sibartlett/Geo/pull/110
[#111]: https://github.com/sibartlett/Geo/pull/111
[#112]: https://github.com/sibartlett/Geo/pull/112
[#113]: https://github.com/sibartlett/Geo/pull/113
[#114]: https://github.com/sibartlett/Geo/pull/114
[#115]: https://github.com/sibartlett/Geo/pull/115
[#116]: https://github.com/sibartlett/Geo/pull/116
[#117]: https://github.com/sibartlett/Geo/pull/117
[#118]: https://github.com/sibartlett/Geo/pull/118
[#119]: https://github.com/sibartlett/Geo/pull/119
[#120]: https://github.com/sibartlett/Geo/pull/120
[#121]: https://github.com/sibartlett/Geo/pull/121
[#122]: https://github.com/sibartlett/Geo/pull/122
[#123]: https://github.com/sibartlett/Geo/pull/123
[#124]: https://github.com/sibartlett/Geo/pull/124
[#125]: https://github.com/sibartlett/Geo/pull/125
[#126]: https://github.com/sibartlett/Geo/pull/126
[#127]: https://github.com/sibartlett/Geo/pull/127
[#128]: https://github.com/sibartlett/Geo/pull/128
[#129]: https://github.com/sibartlett/Geo/pull/129
[2.0.0]: https://github.com/sibartlett/Geo/compare/v1.2.0...v2.0.0
[3.0.0]: https://github.com/sibartlett/Geo/compare/v2.0.0...v3.0.0
