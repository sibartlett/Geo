using System;
using System.Text.RegularExpressions;
using Geo.Geometries;
using Geo.Interfaces;
using Raven.Abstractions.Data;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Geo.Raven
{
    public class GeoIndexCreationTask<TDocument> : GeoIndexCreationTask<TDocument, TDocument>
    {
    }

    public class GeoIndexCreationTask<TDocument, TReduceResult> : AbstractIndexCreationTask<TDocument, TReduceResult>
    {
        public object GeoIndex(IRavenIndexable shape)
        {
            throw new NotSupportedException("This method is provided solely to allow query translation on the server");
        }

        public object GeoIndex(IRavenIndexable shape, SpatialSearchStrategy strategy)
        {
            throw new NotSupportedException("This method is provided solely to allow query translation on the server");
        }

        public object GeoIndex(IRavenIndexable shape, SpatialSearchStrategy strategy, int maxTreeLevel)
        {
            throw new NotSupportedException("This method is provided solely to allow query translation on the server");
        }

        public override IndexDefinition CreateIndexDefinition()
        {
            var definition = base.CreateIndexDefinition();
            definition.Map = TransformGeoIndexes(definition.Map);
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
