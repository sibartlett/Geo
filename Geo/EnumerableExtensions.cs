using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Interfaces;

namespace Geo
{
    public static class EnumerableExtensions
    {
        public static IEnumerable<TSource> Distinct2D<TSource>(this IEnumerable<TSource> source) where TSource : ISpatialEquatable<TSource>
        {
            return source.Distinct(new Spatial2DComparer<TSource>());
        }

        public static IEnumerable<TSource> Distinct3D<TSource>(this IEnumerable<TSource> source) where TSource : ISpatialEquatable<TSource>
        {
            return source.Distinct(new Spatial3DComparer<TSource>());
        }
    }
}
