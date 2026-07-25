#nullable enable
using System;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Geodesy;

public class SphereCalculator : IGeodeticCalculator
{
    public SphereCalculator()
        : this(Constants.EarthMeanRadius) { }

    public SphereCalculator(double radius)
    {
        Radius = radius;
    }

    public double Radius { get; protected set; }

    /// <summary>
    /// The great-circle direct problem: where you arrive setting off from
    /// <paramref name="point" /> on the given <paramref name="heading" /> and holding a
    /// great circle for <paramref name="distance" /> metres.
    /// </summary>
    public GeodeticLine CalculateOrthodromicLine(IPosition point, double heading, double distance)
    {
        var coordinate = point.GetCoordinate();
        var lat1 = coordinate.Latitude.ToRadians();
        var lon1 = coordinate.Longitude.ToRadians();
        var azimuth = heading.ToRadians();
        var angularDistance = distance / Radius;

        var sinLat1 = Math.Sin(lat1);
        var cosLat1 = Math.Cos(lat1);
        var sinAngular = Math.Sin(angularDistance);
        var cosAngular = Math.Cos(angularDistance);

        var lat2 = Math.Asin(sinLat1 * cosAngular + cosLat1 * sinAngular * Math.Cos(azimuth));
        var lon2 =
            lon1
            + Math.Atan2(
                Math.Sin(azimuth) * sinAngular * cosLat1,
                cosAngular - sinLat1 * Math.Sin(lat2)
            );

        return new GeodeticLine(
            new Coordinate(coordinate.Latitude, coordinate.Longitude),
            new Coordinate(ClampLatitude(lat2.ToDegrees()), NormalizeLongitude(lon2).ToDegrees()),
            distance,
            heading,
            // The back azimuth is the heading you would set off on to make the return
            // journey, so it is the initial bearing of the reversed line - in degrees,
            // like every other bearing here.
            InitialBearing(lat2, lon2, lat1, lon1).ToDegrees()
        );
    }

    /// <summary>
    /// The great-circle inverse problem: the shortest path between two points, and the
    /// headings it starts and ends on. Returns <c>null</c> when the two coincide.
    /// </summary>
    public GeodeticLine? CalculateOrthodromicLine(IPosition point1, IPosition point2)
    {
        var result = CalculateOrthodromicLineInternal(point1, point2);
        if (result == null)
            return null;

        return new GeodeticLine(
            point1.GetCoordinate(),
            point2.GetCoordinate(),
            result[0],
            result[1],
            result[2]
        );
    }

    /// <summary>
    /// The rhumb line (line of constant bearing) between two points. Longer than the
    /// great circle, but it can be steered on a single heading. Returns <c>null</c> when
    /// the two points coincide.
    /// </summary>
    public GeodeticLine? CalculateLoxodromicLine(IPosition point1, IPosition point2)
    {
        var coordinate1 = point1.GetCoordinate();
        var coordinate2 = point2.GetCoordinate();

        if (Coincident(coordinate1, coordinate2))
            return null;

        var lat1 = coordinate1.Latitude.ToRadians();
        var lat2 = coordinate2.Latitude.ToRadians();
        var deltaLat = lat2 - lat1;
        var deltaLon = NormalizeLongitude(
            coordinate2.Longitude.ToRadians() - coordinate1.Longitude.ToRadians()
        );

        // A rhumb line is a straight line on a Mercator chart, so it is measured against
        // the stretched (Mercator) latitude rather than the true one.
        var deltaStretched = Math.Log(
            Math.Tan(Math.PI / 4 + lat2 / 2) / Math.Tan(Math.PI / 4 + lat1 / 2)
        );

        // deltaLat / deltaStretched is the north-south scale factor. Due east or west
        // both are zero, and the ratio tends to cos(latitude) - the parallel's own scale.
        var scale = Math.Abs(deltaStretched) > 1e-12 ? deltaLat / deltaStretched : Math.Cos(lat1);

        var distance =
            Radius * Math.Sqrt(deltaLat * deltaLat + scale * scale * deltaLon * deltaLon);
        var course = Math.Atan2(deltaLon, deltaStretched).ToDegrees();

        // The bearing is constant along a rhumb line, so the return heading is simply the
        // reciprocal (GeodeticLine wraps it back into 0-360).
        return new GeodeticLine(coordinate1, coordinate2, distance, course, course + 180);
    }

    public Distance CalculateLength(Circle circle)
    {
        var h = Radius * (1 - Math.Cos(circle.Radius / Radius));
        var circumference = 2 * Math.PI * Math.Sqrt(h * (2 * Radius - h));
        return new Distance(circumference);
    }

    public Distance CalculateLength(CoordinateSequence coordinates)
    {
        var distance = 0d;
        for (var i = 1; i < coordinates.Count; i++)
        {
            var result = CalculateOrthodromicLineInternal(coordinates[i - 1], coordinates[i]);
            if (result != null)
                distance += result[0];
        }

        return new Distance(distance);
    }

    public Distance CalculateLength(Envelope envelope)
    {
        // Perimeter of the envelope: the two east-west arcs along the parallels
        // at the min/max latitude, plus the two north-south meridian arcs at the
        // sides. Fractions are of a full 360 degree great circle.
        var latFraction = (envelope.MaxLat - envelope.MinLat) / 360;
        var lonFraction = (envelope.MaxLon - envelope.MinLon) / 360;

        // Radius of the circle of latitude (parallel) at each latitude.
        var r1 = Radius * Math.Cos(envelope.MaxLat.ToRadians());
        var r2 = Radius * Math.Cos(envelope.MinLat.ToRadians());

        var top = 2 * Math.PI * r1 * lonFraction;
        var bottom = 2 * Math.PI * r2 * lonFraction;
        var sides = 2 * Math.PI * Radius * latFraction * 2;

        return new Distance(top + bottom + sides);
    }

    public Area CalculateArea(CoordinateSequence coordinates)
    {
        var area = 0.0;
        if (coordinates.Count > 3 && coordinates.IsClosed)
        {
            for (var i = 0; i < coordinates.Count - 1; i++)
            {
                var p1 = coordinates[i];
                var p2 = coordinates[i + 1];
                area +=
                    (p2.Longitude - p1.Longitude).ToRadians()
                    * (2 + Math.Sin(p1.Latitude.ToRadians()) + Math.Sin(p2.Latitude.ToRadians()));
            }

            area = area * Radius * Radius / 2.0;
        }

        // The formula yields a signed area whose sign depends on the ring's winding
        // order. Callers (e.g. Polygon.GetArea, which subtracts hole areas from the
        // shell area) expect a non-negative magnitude, so return the absolute value.
        return new Area(Math.Abs(area));
    }

    public Area CalculateArea(Circle circle)
    {
        if (circle.Radius <= 0)
            return new Area(0d);

        if (circle.Radius > Math.PI * Radius)
            return new Area(0d);

        var h = Radius * (1 - Math.Cos(circle.Radius / Radius));
        var area = 2 * Math.PI * Radius * h;
        return new Area(area);
    }

    public Area CalculateArea(Envelope envelope)
    {
        // Area of the spherical zone between the two latitudes, scaled by the
        // fraction of longitude the envelope spans:
        //   2 * pi * R^2 * (sin(maxLat) - sin(minLat)) * (lonSpan / 360).
        var h1 = Radius * (1 - Math.Sin(envelope.MaxLat.ToRadians()));
        var h2 = Radius * (1 - Math.Sin(envelope.MinLat.ToRadians()));
        var zoneArea = 2 * Math.PI * Radius * (h2 - h1);
        var lonPercentage = (envelope.MaxLon - envelope.MinLon) / 360;
        return new Area(zoneArea * lonPercentage);
    }

    /// <summary>
    /// The Mercator ordinate of a latitude, in nautical miles - the same unit
    /// <see cref="SpheroidCalculator.CalculateMeridionalParts" /> reports, so the two are
    /// directly comparable. On a sphere the eccentricity correction a spheroid needs
    /// vanishes and only the isometric latitude remains.
    /// </summary>
    public double CalculateMeridionalParts(double latitude)
    {
        return Radius
            * Math.Log(Math.Tan(Math.PI / 4 + latitude.ToRadians() / 2))
            / Constants.NauticalMile;
    }

    /// <summary>
    /// The distance along the meridian from the equator to a latitude, in metres. A
    /// sphere's meridians are circles of a single radius, so the arc is simply
    /// <see cref="Radius" /> times the latitude in radians.
    /// </summary>
    public double CalculateMeridionalDistance(double latitude)
    {
        return Radius * latitude.ToRadians();
    }

    private double[]? CalculateOrthodromicLineInternal(IPosition position1, IPosition position2)
    {
        var coordinate1 = position1.GetCoordinate();
        var coordinate2 = position2.GetCoordinate();

        if (Coincident(coordinate1, coordinate2))
            return null;

        var lat1 = coordinate1.Latitude.ToRadians();
        var lon1 = coordinate1.Longitude.ToRadians();
        var lat2 = coordinate2.Latitude.ToRadians();
        var lon2 = coordinate2.Longitude.ToRadians();

        // Haversine rather than the spherical law of cosines: the latter loses its
        // precision for points close together, which is where a length is most often
        // summed over many short legs.
        var sinHalfLat = Math.Sin((lat2 - lat1) / 2);
        var sinHalfLon = Math.Sin((lon2 - lon1) / 2);
        var h = sinHalfLat * sinHalfLat + Math.Cos(lat1) * Math.Cos(lat2) * sinHalfLon * sinHalfLon;

        return new[]
        {
            2 * Math.Asin(Math.Min(1, Math.Sqrt(h))) * Radius,
            InitialBearing(lat1, lon1, lat2, lon2).ToDegrees(),
            InitialBearing(lat2, lon2, lat1, lon1).ToDegrees(),
        };
    }

    /// <summary>
    /// The heading, in radians, to set off on at (<paramref name="lat1" />,
    /// <paramref name="lon1" />) to reach (<paramref name="lat2" />,
    /// <paramref name="lon2" />) along a great circle.
    /// </summary>
    private static double InitialBearing(double lat1, double lon1, double lat2, double lon2)
    {
        var deltaLon = lon2 - lon1;
        return Math.Atan2(
            Math.Sin(deltaLon) * Math.Cos(lat2),
            Math.Cos(lat1) * Math.Sin(lat2) - Math.Sin(lat1) * Math.Cos(lat2) * Math.Cos(deltaLon)
        );
    }

    private static bool Coincident(Coordinate coordinate1, Coordinate coordinate2)
    {
        return Math.Abs(coordinate1.Latitude - coordinate2.Latitude) < double.Epsilon
            && Math.Abs(coordinate1.Longitude - coordinate2.Longitude) < double.Epsilon;
    }

    private static double NormalizeLongitude(double radians)
    {
        var turn = 2 * Math.PI;
        return radians - turn * Math.Floor((radians + Math.PI) / turn);
    }

    // Asin is bounded by pi/2, but converting that to degrees can land a hair past 90,
    // which Coordinate rejects outright.
    private static double ClampLatitude(double degrees)
    {
        return Math.Max(-90, Math.Min(90, degrees));
    }
}
