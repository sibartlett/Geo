using System.Collections.Generic;
using System.Linq;
using Geo.Interfaces;

namespace Geo.Json
{
    public class FeatureCollection : IGeoJsonObject
    {
        public FeatureCollection()
        {
            Features = new List<Feature>();
        }

        public FeatureCollection(IEnumerable<Feature> features)
        {
            Features = new List<Feature>(features);
        }

        public FeatureCollection(params Feature[] features)
        {
            Features = new List<Feature>(features);
        }

        public List<Feature> Features { get; private set; }

        public string ToGeoJson()
        {
            return SimpleJson.SerializeObject(ToGeoJsonObject());
        }

        internal object ToGeoJsonObject()
        {
            return new Dictionary<string, object>
            {
                { "type", "FeatureCollection" },
                { "features", Features.Select(x => x.ToGeoJsonObject()).ToArray() }
            };
        }
    }
}
