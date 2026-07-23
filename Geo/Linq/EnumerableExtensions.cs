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
    /// The coordinates are sorted by their bearing from the average of all the vertices,
    /// treating longitude as the x-axis and latitude as the y-axis. This reliably recovers
    /// the boundary order of a <em>convex</em> set of coordinates (for example the corners
    /// of a bounding box), but a concave set has more than one valid ordering and this
    /// method may not reproduce the one you had in mind. The result is a permutation of the
    /// input — it is not closed; append the first coordinate to form a ring.
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
    /// using the signed area (shoelace formula). Longitude is treated as the x-axis and latitude
    /// as the y-axis. The ring may be open or closed; the edge from the last coordinate back to
    /// the first is always included.
    /// </summary>
    /// <param name="coordinates">The ordered ring of coordinates.</param>
    /// <returns>
    /// The <see cref="WindingOrder" /> of the ring, or <c>null</c> when it is degenerate (fewer
    /// than three coordinates, or a zero signed area such as a set of colinear points).
    /// </returns>
    /// <exception cref="ArgumentNullException"><paramref name="coordinates" /> is <c>null</c>.</exception>
    public static WindingOrder? GetWindingOrder(this IEnumerable<Coordinate> coordinates)
    {
        if (coordinates == null)
            throw new ArgumentNullException(nameof(coordinates));

        var list = coordinates as IReadOnlyList<Coordinate> ?? coordinates.ToList();
        if (list.Count < 3)
            return null;

        // Twice the signed area: positive winds counter-clockwise, negative clockwise.
        var twiceArea = 0d;
        for (var i = 0; i < list.Count; i++)
        {
            var current = list[i];
            var next = list[(i + 1) % list.Count];
            twiceArea += current.Longitude * next.Latitude - next.Longitude * current.Latitude;
        }

        if (twiceArea > 0)
            return WindingOrder.CounterClockwise;
        if (twiceArea < 0)
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

        var centerLatitude = list.Average(x => x.Latitude);
        var centerLongitude = list.Average(x => x.Longitude);

        // Bearing from the centroid; longitude is the x-axis and latitude the y-axis, so an
        // ascending atan2 sweeps counter-clockwise. Distance keeps colinear points stable.
        var keyed = list.Select(x =>
        {
            var dx = x.Longitude - centerLongitude;
            var dy = x.Latitude - centerLatitude;
            return (Coordinate: x, Angle: Math.Atan2(dy, dx), Radius: dx * dx + dy * dy);
        });

        var sorted =
            order == WindingOrder.CounterClockwise
                ? keyed.OrderBy(x => x.Angle).ThenBy(x => x.Radius)
                : keyed.OrderByDescending(x => x.Angle).ThenByDescending(x => x.Radius);

        return sorted.Select(x => x.Coordinate).ToList();
    }
}
