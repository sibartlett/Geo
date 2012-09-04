using Geo.Geometries;
using Geo.Measure;
using Geo.Raven;
using Raven.Abstractions.Indexing;

namespace Raven.Client
{
    public static class GeoRavenExtensions
    {
        public const string DefaultGeoFieldName = "__spatial";

        public static IDocumentStore ApplyGeoConventions(this IDocumentStore store)
        {
            store.Conventions.JsonContractResolver = new GeoContractResolver();
            return store;
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, double radiusKm, ILatLngCoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radiusKm, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, Distance radius, ILatLngCoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> RelatesToShape<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, IGeometry shape, SpatialRelation relation, double distanceErrorPct = 0.025) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.RelatesToShape(DefaultGeoFieldName, new GeoValueProvider().GetValue(shape), relation, distanceErrorPct);
        }
        
        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, double radiusKm, ILatLngCoordinate coord)
        {
            return self.WithinRadiusOf(radiusKm, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, Distance radius, ILatLngCoordinate coord)
        {
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryCustomization RelatesToShape(this IDocumentQueryCustomization self, IGeometry shape, SpatialRelation relation)
        {
            return self.RelatesToShape(DefaultGeoFieldName, new GeoValueProvider().GetValue(shape), relation);
        }
    }
}
