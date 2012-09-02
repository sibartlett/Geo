using System;
using System.Text.RegularExpressions;
using Geo.Geometries;
using Raven.Abstractions.Indexing;
using Raven.Client.Indexes;

namespace Geo.Raven
{
    public class GeoIndexCreationTask<TDocument> : GeoIndexCreationTask<TDocument, TDocument>
    {
    }

    public class GeoIndexCreationTask<TDocument, TReduceResult> : AbstractIndexCreationTask<TDocument, TReduceResult>
    {
        public object GeoIndex(ILatLngCoordinate coordinate)
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

            return Regex.Replace(value, @"GeoIndex\((?<prop>[\w\d\s\.]+)\)", match =>
                       string.Format("SpatialGenerate({0}.Latitude, {0}.Longitude)", match.Groups["prop"].Value));
        }
    }
}
