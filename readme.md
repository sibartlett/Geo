### UNSTABLE 0.x VERSION
Anything may change at any time. The public API should not be considered stable. (http://semver.org/)

# Geo - a simple geospatial library for .NET

Geo is a spatial library that:

* is made specfically for geographic data
* uses geographic terminology
* is simple

#### NuGet Packages

__Geo__ - _[NuGet](https://nuget.org/packages/Geo) (.NET 4.0+, Windows 8 Store applications, Windows Phone 7.0+, Silverlight 4 & 5)_

Features include:
* Geographic geometry types:
	* Point
	* LineString
	* Polygon, Triangle
	* Circle
	* GeometryCollection, MultiPoint, MultiLineString, and MultiPolygon
* GPS types:
	* GPSData
	* Route
	* Track
* Serialize and deserialize geometries:
	* Well-Known text
	* GeoJSON
	* Spatial4n/Spatial4j shape strings
* Serialize and deserialize GPS files:
	* GPX
	* NMEA (deserialize only)
	* IGC (deserialize only)
	* Garmin Flightplan (deserialize only)
	* SkyDemon flightplan (deserialize only)
	* PocketFMS flightplan (deserialize only)
* Geographic calculations
	* Distance
	* Area
	* Greate circle lines
	* Rhumb lines

__Geo.Raven__ - _[NuGet](https://nuget.org/packages/Geo.Raven) (.NET 4.0+, Silverlight 4 & 5)_

Features include:
* Define RavenDB indexes against Geo geometries
* Use Geo geometries in RavenDB spatial queries

#### Useful Information

* All ordinates are in degress, unless specified otherwise
* All measurements are in S.I. units (metres, seconds, etc.), unless specified otherwise

#### License

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.