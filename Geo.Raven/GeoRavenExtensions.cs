using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Geo.Geometries;
using Geo.Interfaces;
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

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, double radiusKm, IPosition position) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            var coordinate = position.GetCoordinate();
            return self.WithinRadiusOf(radiusKm, coordinate.Latitude, coordinate.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> WithinRadiusOf<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, Distance radius, IPosition position) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            var coordinate = position.GetCoordinate();
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coordinate.Latitude, coordinate.Longitude);
        }

        public static IDocumentQueryBase<T, TSelf> RelatesToShape<T, TSelf>(this IDocumentQueryBase<T, TSelf> self, IRavenIndexable shape, SpatialRelation relation, double distanceErrorPct = Constants.DefaultSpatialDistanceErrorPct) where TSelf : IDocumentQueryBase<T, TSelf>
        {
            return self.RelatesToShape(Constants.DefaultSpatialFieldName, new GeoValueProvider().GetValue(shape), relation, distanceErrorPct);
        }

        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, double radiusKm, IPosition position)
        {
            var coordinate = position.GetCoordinate();
            return self.WithinRadiusOf(radiusKm, coordinate.Latitude, coordinate.Longitude);
        }

        public static IDocumentQueryCustomization WithinRadiusOf(this IDocumentQueryCustomization self, Distance radius, IPosition position)
        {
            var coordinate = position.GetCoordinate();
            return self.WithinRadiusOf(radius.ConvertTo(DistanceUnit.Km).Value, coordinate.Latitude, coordinate.Longitude);
        }

        public static IDocumentQueryCustomization RelatesToShape(this IDocumentQueryCustomization self, IRavenIndexable shape, SpatialRelation relation)
        {
            return self.RelatesToShape(Constants.DefaultSpatialFieldName, new GeoValueProvider().GetValue(shape), relation);
        }

        public static IndexDefinition TransformGeoMaps(this IndexDefinition definition)
        {
            definition.Maps = new HashSet<string>(definition.Maps.Select(TransformGeoIndexes));
            definition.Reduce = TransformGeoIndexes(definition.Reduce);
            return definition;
        }

        private static string TransformGeoIndexes(string value)
        {
            if (value == null)
                return null;

            return Regex.Replace(value, @"GeoIndex\((?<prop>[\w\d\s\.]+)(?<sep>[,)])", match =>
                       string.Format("SpatialGenerate(\"{0}\", {1}.{2}{3}",
                       Constants.DefaultSpatialFieldName,
                       match.Groups["prop"].Value,
                       GeoContractResolver.IndexProperty,
                       match.Groups["sep"].Value));
        }
    }
}
