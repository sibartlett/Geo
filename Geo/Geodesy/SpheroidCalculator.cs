// References:
// http://williams.best.vwh.net/
// https://github.com/geotools/geotools/blob/master/modules/library/referencing/src/main/java/org/geotools/referencing/datum/DefaultEllipsoid.java

using System;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Geodesy;

public class SpheroidCalculator : IGeodeticCalculator
{
    private readonly SphereCalculator _sphereCalculator = new();

    public SpheroidCalculator()
        : this(Spheroid.Default) { }

    public SpheroidCalculator(Spheroid spheroid)
    {
        Spheroid = spheroid;
    }

    public Spheroid Spheroid { get; }

    public GeodeticLine CalculateOrthodromicLine(IPosition point, double heading, double distance)
    {
        var lat1 = point.Coordinate.Latitude.ToRadians();
        var lon1 = point.Coordinate.Longitude.ToRadians();
        var faz = heading.ToRadians();

        // glat1 initial geodetic latitude in radians N positive
        // glon1 initial geodetic longitude in radians E positive
        // faz forward azimuth in radians
        // s distance in units of a (=nm)

        var EPS = 0.00000000005;
        //var r, tu, sf, cf, b, cu, su, sa, c2a, x, c, d, y, sy, cy, cz, e
        //            var glat2, glon2, baz, f

        if (Math.Abs(Math.Cos(lat1)) < EPS && !(Math.Abs(Math.Sin(faz)) < EPS))
        {
            //alert("Only N-S courses are meaningful, starting at a pole!")
        }

        var r = 1 - Spheroid.Flattening;
        var tu = r * Math.Tan(lat1);
        var sf = Math.Sin(faz);
        var cf = Math.Cos(faz);
        double b;
        if (cf == 0)
            b = 0d;
        else
            b = 2 * Math.Atan2(tu, cf);
        var cu = 1 / Math.Sqrt(1 + tu * tu);
        var su = tu * cu;
        var sa = cu * sf;
        var c2a = 1 - sa * sa;
        var x = 1 + Math.Sqrt(1 + c2a * (1 / (r * r) - 1));
        x = (x - 2) / x;
        var c = 1 - x;
        c = (x * x / 4 + 1) / c;
        var d = (0.375 * x * x - 1) * x;
        tu = distance / (r * Spheroid.EquatorialAxis * c);
        var y = tu;
        c = y + 1;

        double sy = 0,
            cy = 0,
            cz = 0,
            e = 0;

        while (Math.Abs(y - c) > EPS)
        {
            sy = Math.Sin(y);
            cy = Math.Cos(y);
            cz = Math.Cos(b + y);
            e = 2 * cz * cz - 1;
            c = y;
            x = e * cy;
            y = e + e - 1;
            y = (((sy * sy * 4 - 3) * y * cz * d / 6 + x) * d / 4 - cz) * sy * d + tu;
        }

        b = cu * cy * cf - su * sy;
        c = r * Math.Sqrt(sa * sa + b * b);
        d = su * cy + cu * sy * cf;
        var glat2 = modlat(Math.Atan2(d, c));
        c = cu * cy - su * sy * cf;
        x = Math.Atan2(sy * sf, c);
        c = ((-3 * c2a + 4) * Spheroid.Flattening + 4) * c2a * Spheroid.Flattening / 16;
        d = ((e * cy * c + cz) * sy * c + y) * sa;
        var glon2 = modlon(lon1 + x - (1 - c) * d * Spheroid.Flattening); // fix date line problems
        var baz = modcrs(Math.Atan2(sa, b) + Math.PI);

        return new GeodeticLine(
            new Coordinate(point.Coordinate.Latitude, point.Coordinate.Longitude),
            new Coordinate(glat2.ToDegrees(), glon2.ToDegrees()),
            distance,
            heading,
            baz
        );
    }

    public GeodeticLine CalculateOrthodromicLine(IPosition position1, IPosition position2)
    {
        var result = CalculateOrthodromicLineInternal(position1, position2);
        if (result == null)
            return null;
        return new GeodeticLine(
            position1.Coordinate,
            position2.Coordinate,
            result[0],
            result[1],
            result[2]
        );
    }

    public GeodeticLine CalculateLoxodromicLine(IPosition position1, IPosition position2)
    {
        var point1 = position1.Coordinate;
        var point2 = position2.Coordinate;
        var lat1 = point1.Latitude;
        var lon1 = point1.Longitude;
        var lat2 = point2.Latitude;
        var lon2 = point2.Longitude;

        if (Math.Abs(lat1 - lat2) < double.Epsilon && Math.Abs(lon1 - lon2) < double.Epsilon)
            return null;

        double distance;
        var latDeltaRad = (lat2 - lat1).ToRadians();
        var meridionalDistance =
            CalculateMeridionalDistance(lat2) - CalculateMeridionalDistance(lat1);
        var course = LoxodromicLineCourse(lat1, lon1, lat2, lon2);

        if (Math.Abs(latDeltaRad) < 0.0008)
        {
            // near parallel sailing

            var lonDelta = lon2 - lon1;
            if (lonDelta > 180)
                lonDelta = lonDelta - 360;
            if (lonDelta < -180)
                lonDelta = lonDelta + 360;
            var lonDeltaRad = lonDelta.ToRadians();

            var midLatRad = (0.5 * (lat1 + lat2)).ToRadians();
            // expand merid_dist/dmp about lat_mid_rad to order e2*dlat_rad^2
            var e2 = Math.Pow(Spheroid.Eccentricity, 2);
            var ratio =
                Math.Cos(midLatRad)
                / Math.Sqrt(1 - e2 * Math.Pow(Math.Sin(midLatRad), 2))
                * (
                    1.0
                    + (
                        e2 * Math.Cos(2 * midLatRad) / 8
                        - (1 + 2 * Math.Pow(Math.Tan(midLatRad), 2)) / 24
                        - e2 / 12
                    )
                        * latDeltaRad
                        * latDeltaRad
                );

            distance = Math.Sqrt(
                Math.Pow(meridionalDistance, 2)
                    + Math.Pow(Spheroid.EquatorialAxis * ratio * lonDeltaRad, 2)
            );
        }
        else
        {
            distance = Math.Abs(meridionalDistance / Math.Cos(course.ToRadians()));
        }

        return new GeodeticLine(
            new Coordinate(lat1, lon1),
            new Coordinate(lat2, lon2),
            distance,
            course,
            course > 180 ? course - 180 : course + 180
        );
    }

    public Distance CalculateLength(Circle circle)
    {
        return _sphereCalculator.CalculateLength(circle);
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
        return _sphereCalculator.CalculateLength(envelope);
    }

    public double CalculateMeridionalParts(double latitude)
    {
        var lat = latitude.ToRadians();
        var a =
            Spheroid.EquatorialAxis
            * (
                Math.Log(Math.Tan(0.5 * lat + Math.PI / 4.0))
                + Spheroid.Eccentricity
                    / 2.0
                    * Math.Log(
                        (1 - Spheroid.Eccentricity * Math.Sin(lat))
                            / (1 + Spheroid.Eccentricity * Math.Sin(lat))
                    )
            );
        return a / Constants.NauticalMile;
    }

    public double CalculateMeridionalDistance(double latitude)
    {
        var lat = latitude.ToRadians();
        var e2 = Math.Pow(Spheroid.Eccentricity, 2);
        var b0 = 1 - e2 / 4 * (1 + e2 / 16 * (3 + 5 * e2 / 4 * (1 + 35 * e2 / 64)));
        var b2 = -(3 / 8.0) * (1 + e2 / 4 * (1 + 15 * e2 / 32 * (1 + 7 * e2 / 12)));
        var b4 = 15 / 256.0 * (1 + 3 * e2 / 4 * (1 + 35 * e2 / 48));
        var b6 = -(35 / 3072.0) * (1 + 5 * e2 / 4);
        const double b8 = 315 / 131072.0;
        var dist =
            b0 * lat
            + e2
                * (
                    b2 * Math.Sin(2 * lat)
                    + e2
                        * (
                            b4 * Math.Sin(4 * lat)
                            + e2 * (b6 * Math.Sin(6 * lat) + e2 * (b8 * Math.Sin(8 * lat)))
                        )
                );
        return dist * Spheroid.EquatorialAxis;
    }

    public Area CalculateArea(CoordinateSequence coordinates)
    {
        return _sphereCalculator.CalculateArea(coordinates);
    }

    public Area CalculateArea(Circle circle)
    {
        return _sphereCalculator.CalculateArea(circle);
    }

    public Area CalculateArea(Envelope envelope)
    {
        return _sphereCalculator.CalculateArea(envelope);
    }

    private double mod(double x, double y)
    {
        return x - y * Math.Floor(x / y);
    }

    private double modlon(double x)
    {
        return mod(x + Math.PI, 2 * Math.PI) - Math.PI;
    }

    private double modcrs(double x)
    {
        return mod(x, 2 * Math.PI);
    }

    private double modlat(double x)
    {
        return mod(x + Math.PI / 2, 2 * Math.PI) - Math.PI / 2;
    }

    private double[] CalculateOrthodromicLineInternal(IPosition position1, IPosition position2)
    {
        var point1 = position1.Coordinate;
        var point2 = position2.Coordinate;

        if (
            Math.Abs(point1.Latitude - point2.Latitude) < double.Epsilon
            && Math.Abs(point1.Longitude - point2.Longitude) < double.Epsilon
        )
            return null;

        var lon1 = point1.Longitude.ToRadians();
        var lat1 = point1.Latitude.ToRadians();
        var lon2 = point2.Longitude.ToRadians();
        var lat2 = point2.Latitude.ToRadians();
        /*
         * Solution of the geodetic inverse problem after T.Vincenty.
         * Modified Rainsford's method with Helmert's elliptical terms.
         * Effective in any azimuth and at any distance short of antipodal.
         *
         * Latitudes and longitudes in radians positive North and East.
         * Forward azimuths at both points returned in radians from North.
         *
         * Programmed for CDC-6600 by LCDR L.Pfeifer NGS ROCKVILLE MD 18FEB75
         * Modified for IBM SYSTEM 360 by John G.Gergen NGS ROCKVILLE MD 7507
         * Ported from Fortran to Java by Martin Desruisseaux.
         *
         * Source: ftp://ftp.ngs.noaa.gov/pub/pcsoft/for_inv.3d/source/inverse.for
         *         subroutine INVER1
         */
        const int maxIterations = 100;
        const double eps = 0.5E-13;
        var R = 1 - Spheroid.Flattening;

        var tu1 = R * Math.Sin(lat1) / Math.Cos(lat1);
        var tu2 = R * Math.Sin(lat2) / Math.Cos(lat2);
        var cu1 = 1 / Math.Sqrt(tu1 * tu1 + 1);
        var cu2 = 1 / Math.Sqrt(tu2 * tu2 + 1);
        var su1 = cu1 * tu1;
        var s = cu1 * cu2;
        var baz = s * tu2;
        var faz = baz * tu1;
        var x = lon2 - lon1;
        var x2 = double.MinValue;
        var x2b = false; // has not converged, but is flip-flopping
        for (var i = 0; i < maxIterations; i++)
        {
            var sx = Math.Sin(x);
            var cx = Math.Cos(x);
            tu1 = cu2 * sx;
            tu2 = baz - su1 * cu2 * cx;
            var sy = Math.Sqrt(Math.Pow(tu1, 2) + Math.Pow(tu2, 2));
            var cy = s * cx + faz;
            var y = Math.Atan2(sy, cy);
            var SA = s * sx / sy;
            var c2a = 1 - SA * SA;
            var cz = faz + faz;
            if (c2a > 0)
                cz = -cz / c2a + cy;
            var e = cz * cz * 2 - 1;
            var c = ((-3 * c2a + 4) * Spheroid.Flattening + 4) * c2a * Spheroid.Flattening / 16;
            var d = x;

            x = ((e * cy * c + cz) * sy * c + y) * SA;
            x = (1 - c) * x * Spheroid.Flattening + lon2 - lon1;

            if (Math.Abs(x - x2) <= double.Epsilon)
            {
                d = x = (x + d) / 2;
                x2b = true;
                continue;
            }

            x2 = d;

            var com = Math.Abs(d - x);
            if (x2b || com <= eps)
            {
                x = Math.Sqrt((1 / (R * R) - 1) * c2a + 1) + 1;
                x = (x - 2) / x;
                c = 1 - x;
                c = (x * x / 4 + 1) / c;
                d = (0.375 * x * x - 1) * x;
                x = e * cy;
                s = 1 - 2 * e;
                s =
                    ((((sy * sy * 4 - 3) * s * cz * d / 6 - x) * d / 4 + cz) * sy * d + y)
                    * c
                    * R
                    * Spheroid.EquatorialAxis;
                // 'faz' and 'baz' are forward azimuths at both points.
                faz = Math.Atan2(tu1, tu2);
                baz = Math.Atan2(cu1 * sx, baz * cx - su1 * cu2) + Math.PI;
                return new[] { s, faz.ToDegrees(), baz.ToDegrees() };
            }
        }

        // No convergence. It may be because coordinate points
        // are equals or because they are at antipodes.
        const double leps = 1E-10;
        if (Math.Abs(lon1 - lon2) <= leps && Math.Abs(lat1 - lat2) <= leps)
            // Coordinate points are equals
            return null;
        if (Math.Abs(lat1) <= leps && Math.Abs(lat2) <= leps)
            // Points are on the equator.
            return new[]
            {
                Math.Abs(lon1 - lon2) * Spheroid.EquatorialAxis,
                faz.ToDegrees(),
                baz.ToDegrees(),
            };
        // Other cases: no solution for this algorithm.
        throw new ArithmeticException();
    }

    private double LoxodromicLineCourse(double lat1, double lon1, double lat2, double lon2)
    {
        var mpDelta = CalculateMeridionalParts(lat2) - CalculateMeridionalParts(lat1);
        var latDelta = lat2 - lat1;
        var lonDelta = lon2 - lon1;
        if (lonDelta > 180)
            lonDelta -= 360;
        if (lonDelta < -180)
            lonDelta += 360;

        var lonDeltaRad = lonDelta.ToRadians();

        // Calculate course and distance
        var course = Math.Atan(
            Spheroid.EquatorialAxis / Constants.NauticalMile * lonDeltaRad / mpDelta
        );
        var courseDeg = course.ToDegrees();

        if (latDelta >= 0)
            courseDeg = courseDeg + 360;
        if (latDelta < 0)
            courseDeg = courseDeg + 180;
        if (courseDeg >= 360)
            courseDeg = courseDeg - 360;
        return courseDeg;
    }
}
