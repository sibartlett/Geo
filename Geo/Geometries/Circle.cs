#nullable enable
using System;
using System.Collections.Generic;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Geometries;

public class Circle : Geometry, ISurface
{
    public static Circle Empty => new();

    public Circle()
    {
        Center = null;
        Radius = 0;
    }

    public Circle(Coordinate center, double radius)
    {
        Center = center;
        Radius = radius;
    }

    public Circle(double latitiude, double longitude, double radius)
    {
        Center = new Coordinate(latitiude, longitude);
        Radius = radius;
    }

    public Circle(double latitiude, double longitude, double elevation, double radius)
    {
        Center = new CoordinateZ(latitiude, longitude, elevation);
        Radius = radius;
    }

    public Circle(
        double latitiude,
        double longitude,
        double elevation,
        double measure,
        double radius
    )
    {
        Center = new CoordinateZM(latitiude, longitude, elevation, measure);
        Radius = radius;
    }

    public Coordinate? Center { get; }
    public double Radius { get; }

    public override Envelope? GetBounds()
    {
        if (Center == null)
            return null;

        var center = Center;
        var latitudinalRadiusDeg = Math.Abs(Radius) / (Constants.NauticalMile * 60);

        var minLat = center.Latitude - latitudinalRadiusDeg;
        var maxLat = center.Latitude + latitudinalRadiusDeg;

        // A circle that reaches a pole has no bounded longitude span - every meridian
        // runs through it - and its latitudes run past the pole, which is not a position
        // any coordinate can hold. Clamp to the pole and span the whole range instead of
        // handing back an envelope no coordinate could ever sit inside.
        if (minLat <= -90 || maxLat >= 90)
            return new Envelope(Math.Max(minLat, -90), -180, Math.Min(maxLat, 90), 180);

        // A degree of longitude spans metresPerDegree * cos(latitude) metres, so the
        // parallels converge towards the poles and the east-west extent of the box grows
        // with latitude. The widest meridians the circle touches are the ones tangent to
        // it, at asin(sin r / cos lat) from the centre - not r / cos(lat), which is only
        // its small-angle approximation and understates the box near the poles.
        var longitudinalRadiusDeg = Math.Asin(
                Math.Sin(latitudinalRadiusDeg.ToRadians()) / Math.Cos(center.Latitude.ToRadians())
            )
            .ToDegrees();

        var minLon = center.Longitude - longitudinalRadiusDeg;
        var maxLon = center.Longitude + longitudinalRadiusDeg;

        // An envelope's longitudes run west to east, so it cannot describe a box that
        // wraps across the anti-meridian. A circle that does gets the whole range, which
        // still contains it, rather than a box that silently excludes half of it.
        if (minLon < -180 || maxLon > 180)
            return new Envelope(minLat, -180, maxLat, 180);

        return new Envelope(minLat, minLon, maxLat, maxLon);
    }

    public Area GetArea()
    {
        return GeoContext.Current.GeodeticCalculator.CalculateArea(this);
    }

    public override bool IsEmpty => Center == null;

    public override bool Is3D => Center != null && Center.Is3D;

    public override bool IsMeasured => Center != null && Center.IsMeasured;

    public Distance GetLength()
    {
        return GeoContext.Current.GeodeticCalculator.CalculateLength(this);
    }

    public Polygon ToPolygon(int sides = 36)
    {
        if (sides < 3)
            throw new ArgumentOutOfRangeException("sides", "Must be greater than 2.");

        // An empty circle has no centre to project the vertices from, so it becomes
        // the empty polygon rather than dereferencing the centre. This keeps the
        // WKT/WKB/GeoJSON writers, which convert circles to polygons, from throwing.
        if (IsEmpty)
            return Polygon.Empty;

        var angle = -360d / sides;
        var coordinates = new List<Coordinate>();
        Coordinate? first = null;
        for (var i = 0; i < sides; i++)
        {
            var coord = GeoContext
                .Current.GeodeticCalculator.CalculateOrthodromicLine(Center!, angle * i, Radius)
                .Coordinate2;
            if (i == 0)
                first = coord;
            coordinates.Add(coord);
        }

        coordinates.Add(first!);
        return new Polygon(new LinearRing(coordinates));
    }

    #region Equality methods

    public override bool Equals(object? obj, SpatialEqualityOptions options)
    {
        var other = obj as Circle;
        return !ReferenceEquals(null, other)
            && Radius.Equals(other.Radius)
            && Equals(Center, other.Center, options);
    }

    public override bool Equals(object? obj)
    {
        return base.Equals(obj);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            return (Radius.GetHashCode() * 397)
                ^ (Center != null ? Center.GetHashCode(options) : 0);
        }
    }

    public static bool operator ==(Circle? left, Circle? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(Circle? left, Circle? right)
    {
        return !(left == right);
    }

    #endregion
}
