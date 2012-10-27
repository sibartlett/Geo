using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;

namespace Geo.Json
{
    public static class GeoJsonExtensions
    {
        internal static double[] ToCoordinateArray<T>(this LatLngBase<T> point) where T : LatLngBase<T>
        {
            return point.Elevation.HasValue
                ? new[] { point.Longitude, point.Latitude, point.Elevation.Value }
                : new[] { point.Longitude, point.Latitude };
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