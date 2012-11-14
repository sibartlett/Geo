using System.Collections.Generic;
using Geo.Interfaces;

namespace Geo.IO.GeoJson
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
            return new GeoJsonWriter().Write(this);
        }
    }
}
