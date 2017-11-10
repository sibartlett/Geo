using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo.Linq
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Distinct2D<TSource>(this IEnumerable<TSource> source) where TSource : ISpatialEquatable
        {
            return source.Distinct(new Spatial2DComparer<TSource>());
        }

        public static IEnumerable<TSource> Distinct3D<TSource>(this IEnumerable<TSource> source) where TSource : ISpatialEquatable
        {
            return source.Distinct(new Spatial3DComparer<TSource>());
        }

        public static Area Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
        {
            return new Area(source.Select(selector).Sum(x => x.SiValue));
        }

        public static Distance Sum<TSource>(this IEnumerable<TSource> source, Func<TSource, Distance> selector)
        {
            return new Distance(source.Select(selector).Sum(x => x.SiValue));
        }

        public static Area Max<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
        {
            return new Area(source.Select(selector).Max(x => x.SiValue));
        }

        public static Distance Max<TSource>(this IEnumerable<TSource> source, Func<TSource, Distance> selector)
        {
            return new Distance(source.Select(selector).Max(x => x.SiValue));
        }

        public static Area Min<TSource>(this IEnumerable<TSource> source, Func<TSource, Area> selector)
        {
            return new Area(source.Select(selector).Min(x => x.SiValue));
        }

        public static Distance Min<TSource>(this IEnumerable<TSource> source, Func<TSource, Distance> selector)
        {
            return new Distance(source.Select(selector).Min(x => x.SiValue));
        }
    }
}
