using System.Collections.Generic;
using Geo.Interfaces;

namespace Geo.Json
{
    public class Feature : IGeoJsonObject
    {
        public Feature(IGeoJsonGeometry geometry, Dictionary<string, object> properties = null)
        {
            Geometry = geometry;
            Properties = properties ?? new Dictionary<string, object>();
        }

        public IGeoJsonGeometry Geometry { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public object Id { get; set; }

        public string ToGeoJson()
        {
            return SimpleJson.SerializeObject(this.ToGeoJsonObject());
        }

        internal object ToGeoJsonObject()
        {
            var result = new Dictionary<string, object>
            {
                { "type", "Feature" },
                { "geometry", Geometry.ToGeoJsonObject() }
            };

            if (Properties != null && Properties.Count > 0)
                result.Add("properties", Properties);

            if (Id != null)
                result.Add("id", Id);

            return result;
        }
    }
}
