# Geo - a geospatial library for .NET

[![NuGet](https://img.shields.io/nuget/dt/Geo.svg)](https://nuget.org/packages/Geo)


Geo is a spatial library that is made specfically for geographic data.

[Wiki](https://github.com/sibartlett/Geo/wiki) | [Issues](https://github.com/sibartlett/Geo/issues) | [NuGet](https://nuget.org/packages/Geo)

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

#### Useful Information

* All ordinates are in degress, unless specified otherwise
* All measurements are in S.I. units (metres, seconds, etc.), unless specified otherwise
* The coordinate reference system is assumed to be WGS-84

#### License

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.
