Geo - a simple geospatial library for .NET
==========================================

Why Geo?
--------

Geo is a spatial library that:

* is made specfically for geographic data
* uses geographic terminology
* is simple

What does Geo do?
-----------------

* Provides geographic data types
* Performs geodesic calculations, such as calculating great circle lines and rhumb lines
* Reads GPS files/streams (GPX, IGC, NMEA)
* Writes GPX files/streams
* Integrates with RavenDB client (using the Geo.Raven package)

Geo doesn't have the features I need?
-------------------------------------

I add data types and/or features when I need them, or when I have the time.

I am taking contributes, but would appreciate discussing your idea(s) first. Please raise a ticket to kick off a discussion.

If Geo does not meet your needs then you should checkout the following libraries:

* NetTopologySuite ([Homepage](http://code.google.com/p/nettopologysuite/), [NuGet](http://nuget.org/profiles/nettopologysuite%20-%20team))
* DotSpatial ([Homepage](http://dotspatial.codeplex.com/), [NuGet](http://nuget.org/profiles/mudnug))

Useful Information
------------------

* All ordinates are in degress, unless specified otherwise
* All measurements are in S.I. units (metres, seconds, etc.), unless specified otherwise

License
-------

Geo is licensed under the terms of the GNU Lesser General Public License as published by the Free Software Foundation.