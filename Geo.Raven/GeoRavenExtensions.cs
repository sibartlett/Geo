using Geo.Geometries;
using Geo.Measure;
using Geo.Raven;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;

namespace Raven.Client
{
    public static class GeoRavenExtensions
    {
        public static IDocumentStore ApplyGeoConventions(this IDocumentStore store)
        {
            store.Conventions.JsonContractResolver = new GeoContractResolver();
            return store;
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, double radiusKm, ICoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radiusKm, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, Distance radius, ICoordinate coord) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> RelatesToShape<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, IGeometry shape, SpatialRelation relation, double distanceErrorPct = Constants.DefaultSpatialDistanceErrorPct) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.RelatesToShape(Constants.DefaultSpatialFieldName, new GeoValueProvider().GetValue(shape), relation, distanceErrorPct);
        }
        
        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, double radiusKm, ICoordinate coord)
        {
            return self.WithinRadiusOf(radiusKm, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, Distance radius, ICoordinate coord)
        {
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coord.Latitude, coord.Longitude);
        }

        public static IDocumentQueryCustomization RelatesToShape(this IDocumentQueryCustomization self, IGeometry shape, SpatialRelation relation)
        {
            return self.RelatesToShape(Constants.DefaultSpatialFieldName, new GeoValueProvider().GetValue(shape), relation);
        }
    }
}
