#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Linq;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> Distinct2D<TSource>(this IEnumerable<TSource> source)
        where TSource : ISpatialEquatable
    {
        return source.Distinct(new Spatial2DComparer<TSource>());
    }

    public static IEnumerable<TSource> Distinct3D<TSource>(this IEnumerable<TSource> source)
        where TSource : ISpatialEquatable
    {
        return source.Distinct(new Spatial3DComparer<TSource>());
    }

    public static Area Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
    {
        return new Area(source.Select(selector).Sum(x => x.SiValue));
    }

    public static Distance Sum<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, Distance> selector
    )
    {
        return new Distance(source.Select(selector).Sum(x => x.SiValue));
    }

    public static Area Max<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
    {
        return new Area(source.Select(selector).Max(x => x.SiValue));
    }

    public static Distance Max<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, Distance> selector
    )
    {
        return new Distance(source.Select(selector).Max(x => x.SiValue));
    }

    public static Area Min<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
    {
        return new Area(source.Select(selector).Min(x => x.SiValue));
    }

    public static Distance Min<TSource>(
        this IEnumerable<TSource> source,
        Func<TSource, Distance> selector
    )
    {
        return new Distance(source.Select(selector).Min(x => x.SiValue));
    }

    /// <summary>
    /// Orders an unsorted set of coordinates clockwise around their centroid, so they
    /// trace the boundary of a simple polygon that can be closed into a ring.
    /// </summary>
    /// <remarks>
    /// The coordinates are projected onto the unit sphere and sorted by their azimuth in
    /// the tangent plane at the centroid (the normalised sum of the vertices), so the result
    /// is correct across the antimeridian and around the poles. This reliably recovers the
    /// boundary order of a <em>convex</em> set of coordinates (for example the corners of a
    /// bounding box), but a concave set has more than one valid ordering and this method may
    /// not reproduce the one you had in mind. The result is a permutation of the input — it
    /// is not closed; append the first coordinate to form a ring.
    /// </remarks>
    /// <param name="coordinates">The coordinates to order.</param>
    /// <returns>The coordinates ordered clockwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="coordinates" /> is <c>null</c>.</exception>
    public static IReadOnlyList<Coordinate> OrderClockwise(this IEnumerable<Coordinate> coordinates)
    {
        return OrderRadially(coordinates, WindingOrder.Clockwise);
    }

    /// <summary>
    /// Orders an unsorted set of coordinates counter-clockwise around their centroid, so
    /// they trace the boundary of a simple polygon that can be closed into a ring.
    /// </summary>
    /// <remarks>
    /// See <see cref="OrderClockwise" /> for how the ordering is derived and its limitations
    /// for concave sets.
    /// </remarks>
    /// <param name="coordinates">The coordinates to order.</param>
    /// <returns>The coordinates ordered counter-clockwise.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="coordinates" /> is <c>null</c>.</exception>
    public static IReadOnlyList<Coordinate> OrderCounterClockwise(
        this IEnumerable<Coordinate> coordinates
    )
    {
        return OrderRadially(coordinates, WindingOrder.CounterClockwise);
    }

    /// <summary>
    /// Determines whether an ordered ring of coordinates winds clockwise or counter-clockwise,
    /// as seen from outside the sphere. The ring is measured on the unit sphere via its signed
    /// spherical area, so the antimeridian and the poles (including rings that enclose a pole)
    /// are handled correctly. The ring may be open or closed; the edge from the last coordinate
    /// back to the first is always included.
    /// </summary>
    /// <param name="coordinates">The ordered ring of coordinates.</param>
    /// <returns>
    /// The <see cref="WindingOrder" /> of the ring, or <c>null</c> when it is degenerate (fewer
    /// than three coordinates, a ring that encloses no area such as a set of points on a great
    /// circle, or vertices spread over more than a hemisphere with no well-defined centre).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="coordinates" /> is <c>null</c>.</exception>
    public static WindingOrder? GetWindingOrder(this IEnumerable<Coordinate> coordinates)
    {
        if (coordinates == null)
            throw new ArgumentNullException(nameof(coordinates));

        var list = coordinates as IReadOnlyList<Coordinate> ?? coordinates.ToList();
        if (list.Count < 3)
            return null;

        var vertices = new Vector3[list.Count];
        var sum = Vector3.Zero;
        for (var i = 0; i < list.Count; i++)
        {
            vertices[i] = Vector3.FromCoordinate(list[i]);
            sum = sum.Add(vertices[i]);
        }

        // No well-defined centre to fan from (e.g. vertices spread over more than a hemisphere).
        if (sum.Length < Epsilon)
            return null;

        // Signed spherical area seen from outside the sphere: positive winds counter-clockwise.
        var signedArea = SignedSphericalArea(vertices, sum.Normalize());

        if (signedArea > Epsilon)
            return WindingOrder.CounterClockwise;
        if (signedArea < -Epsilon)
            return WindingOrder.Clockwise;
        return null;
    }

    private static IReadOnlyList<Coordinate> OrderRadially(
        IEnumerable<Coordinate> coordinates,
        WindingOrder order
    )
    {
        if (coordinates == null)
            throw new ArgumentNullException(nameof(coordinates));

        var list = coordinates.ToList();
        if (list.Count < 3)
            return list;

        var vertices = new Vector3[list.Count];
        var sum = Vector3.Zero;
        for (var i = 0; i < list.Count; i++)
        {
            vertices[i] = Vector3.FromCoordinate(list[i]);
            sum = sum.Add(vertices[i]);
        }

        // Direction to sort around. If the vertices cancel out (spread over more than a
        // hemisphere) there is no meaningful centre, so fall back to the first vertex.
        var center = sum.Length < Epsilon ? vertices[0] : sum.Normalize();

        // A tangent basis (u, v) at the centre. (u, v, center) is right-handed, so an
        // ascending azimuth sweeps counter-clockwise as seen from outside the sphere.
        var helper = Math.Abs(center.Z) < 0.9 ? Vector3.UnitZ : Vector3.UnitX;
        var u = helper.Add(center.Scale(-helper.Dot(center))).Normalize();
        var v = center.Cross(u);

        var keyed = new (Coordinate Coordinate, double Angle, double Height)[list.Count];
        for (var i = 0; i < list.Count; i++)
        {
            var p = vertices[i];
            // Azimuth around the centre; height keeps colinear points in a stable order.
            keyed[i] = (list[i], Math.Atan2(p.Dot(v), p.Dot(u)), p.Dot(center));
        }

        var sorted =
            order == WindingOrder.CounterClockwise
                ? keyed.OrderBy(x => x.Angle).ThenByDescending(x => x.Height)
                : keyed.OrderByDescending(x => x.Angle).ThenBy(x => x.Height);

        return sorted.Select(x => x.Coordinate).ToList();
    }

    // Below this, values smaller than a rounding error are treated as zero.
    private const double Epsilon = 1e-12;

    /// <summary>
    /// The signed area of the spherical polygon <paramref name="vertices" />, as the sum of the
    /// signed solid angles of the triangles (<paramref name="reference" />, V_i, V_i+1) using the
    /// Van Oosterom–Strackee formula. Signed areas are additive, so fanning from the reference
    /// direction gives the whole ring regardless of its shape or whether it encloses a pole.
    /// Positive is counter-clockwise as seen from outside the sphere.
    /// </summary>
    private static double SignedSphericalArea(IReadOnlyList<Vector3> vertices, Vector3 reference)
    {
        var total = 0d;
        for (var i = 0; i < vertices.Count; i++)
        {
            var a = vertices[i];
            var b = vertices[(i + 1) % vertices.Count];
            var numerator = reference.Dot(a.Cross(b));
            var denominator = 1 + a.Dot(b) + b.Dot(reference) + reference.Dot(a);
            total += 2 * Math.Atan2(numerator, denominator);
        }

        return total;
    }

    private readonly struct Vector3
    {
        public static readonly Vector3 Zero = new(0, 0, 0);
        public static readonly Vector3 UnitX = new(1, 0, 0);
        public static readonly Vector3 UnitZ = new(0, 0, 1);

        public Vector3(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }

        public double X { get; }
        public double Y { get; }
        public double Z { get; }

        public double Length => Math.Sqrt(Dot(this));

        public static Vector3 FromCoordinate(Coordinate coordinate)
        {
            var latitude = coordinate.Latitude * Math.PI / 180.0;
            var longitude = coordinate.Longitude * Math.PI / 180.0;
            var cosLatitude = Math.Cos(latitude);
            return new Vector3(
                cosLatitude * Math.Cos(longitude),
                cosLatitude * Math.Sin(longitude),
                Math.Sin(latitude)
            );
        }

        public double Dot(Vector3 other) => X * other.X + Y * other.Y + Z * other.Z;

        public Vector3 Cross(Vector3 other) =>
            new(Y * other.Z - Z * other.Y, Z * other.X - X * other.Z, X * other.Y - Y * other.X);

        public Vector3 Add(Vector3 other) => new(X + other.X, Y + other.Y, Z + other.Z);

        public Vector3 Scale(double scalar) => new(X * scalar, Y * scalar, Z * scalar);

        public Vector3 Normalize()
        {
            var length = Length;
            return length == 0 ? this : new Vector3(X / length, Y / length, Z / length);
        }
    }
}
