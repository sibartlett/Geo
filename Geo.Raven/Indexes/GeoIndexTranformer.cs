using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Raven.Abstractions.Indexing;
using Raven.Client.Document;

namespace Geo.Raven.Indexes
{
    internal class GeoIndexTranformer
    {
        public static IndexDefinition Transform(IndexDefinition definition, DocumentConvention conventions)
        {
            definition.Maps = new HashSet<string>(definition.Maps.Select(x => TransformGeoIndexes(x, definition, conventions)));
            definition.Reduce = TransformGeoIndexes(definition.Reduce, definition, conventions);
            return definition;
        }

        private static string TransformGeoIndexes(string value, IndexDefinition definition, DocumentConvention conventions)
        {
            if (value == null)
                return null;

            return Regex.Replace(value, @"GeoIndex\((?<pre>[^.]+)[.](?<prop>[^),]+)(?<remainder>[^)]*)[)]", match =>
            {
                var fieldPrefix = match.Groups["prop"].Value.Replace(".", "_");
                return string.Format("SpatialGenerate(\"{0}_{1}\", {2}.{3}.{1}{4})",
                        match.Groups["prop"].Value.Replace(".", "_"),
                        SpatialField.Name,
                        match.Groups["pre"].Value,
                        match.Groups["prop"].Value,
                        match.Groups["remainder"].Value
                    );
            });
        }
    }
}
