using Geo.Geometries;
using Geo.Measure;
using Geo.Raven;

namespace Raven.Client
{
    public static class GeoRavenExtensions
    {
        public static IDocumentStore ApplyGeoConventions(this IDocumentStore store)
        {
            store.Conventions.JsonContractResolver = new GeoContractResolver();
            return store;
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, double radiusMiles, ILatLngCoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radiusMiles, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, Distance radius, ILatLngCoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Mile).Value, coord.Latitude, coord.Longitude);
        }
    }
}
