using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Interfaces;

namespace Geo.Json
{
    public static class GeoJsonExtensions
    {
        internal static double[] ToCoordinateArray(this IPosition position)
        {
            var point = position.GetCoordinate();
            if (point.HasElevation && point.HasM)
                return new[] {point.Longitude, point.Latitude, point.Elevation, point.M};
            if (point.HasElevation)
                return new[] {point.Longitude, point.Latitude, point.Elevation};
            if (point.HasM)
                return new[] {point.Longitude, point.Latitude, point.M};
            return new[] {point.Longitude, point.Latitude};
        }

        internal static IEnumerable<double[]> ToCoordinateArray(this CoordinateSequence sequence)
        {
            return sequence.Select(x => x.ToCoordinateArray()).ToArray();
        }

        internal static IEnumerable<IEnumerable<double[]>> ToCoordinateArray(this Polygon polygon)
        {
            yield return polygon.Shell.Coordinates.ToCoordinateArray();
            foreach (var hole in polygon.Holes)
                yield return hole.Coordinates.ToCoordinateArray();
        }
    }
}