# Geo - a simple geospatial library for .NET

Geo is a spatial library that:

* is made specfically for geographic data
* uses geographic terminology
* is simple

#### NuGet Packages

__Geo__ - _[NuGet](https://nuget.org/packages/Geo)_

* Features:
	* Provides geographic data types (Point, LineString, Polygon, Circle, etc.)
	* Performs geodesic calculations, such as calculating great circle lines and rhumb lines
	* Read GPS data from a .NET Stream;
		* GPX
		* NMEA
		* IGC
		* Garmin Flightplan
		* SkyDemon flightplan
		* PocketFMS flightplan
	* Write GPX file to a .NET Stream
* .NET 4.0+, Windows 8 Store applications, Windows Phone 7.0+, Silverlight 4 & 5

__Geo.Raven__ - _[NuGet](https://nuget.org/packages/Geo.Raven)_

* Features:
	* Define RavenDB indexes against Geo geometries
	* Use Geo geometries in RavenDB spatial queries
* .NET 4.0+, Silverlight 4 & 5

#### Useful Information

* All ordinates are in degress, unless specified otherwise
* All measurements are in S.I. units (metres, seconds, etc.), unless specified otherwise

#### License

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.