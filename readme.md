# Geo - a geospatial library for .NET

Geo is a spatial library that is made specfically for geographic data.

[Wiki](https://github.com/sibartlett/Geo/wiki) | [Issues](https://github.com/sibartlett/Geo/issues)

#### NuGet Packages

__Geo__ - _[NuGet](https://nuget.org/packages/Geo) (.NET 4.0+, Windows 8, Windows Phone 8.0+, Silverlight 5, Xamarin Android and iOS)_

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
	* WKT (Well-known text)
	* WKB (Well-known binary)
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
* Geomagnetism calculations
	* IGRF / WMM models
	* Declination, Inclination, Intensity, etc.

__Geo.Raven__ - _[NuGet](https://nuget.org/packages/Geo.Raven) (.NET 4.0+)_

Features include:
* Define RavenDB indexes against Geo geometries
* Use Geo geometries in RavenDB spatial queries

__RavenDB JsonConverters for Geo__ - _[NuGet](https://www.nuget.org/packages/RavenDB.Client.Spatial.Geo) (.NET 4.0+, Silverlight 5)_

RavenDB JsonConverters, which let you take advantage of the new spatial features in Raven 2.5.

#### Useful Information

* All ordinates are in degress, unless specified otherwise
* All measurements are in S.I. units (metres, seconds, etc.), unless specified otherwise
* The coordinate reference system is assumed to be WGS-84

#### License

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.
