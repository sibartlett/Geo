using System.Collections.Generic;
using Geo.Abstractions.Interfaces;

namespace Geo.IO.GeoJson
{
    public class Feature : IGeoJsonObject
    {
        public Feature(IGeometry geometry, Dictionary<string, object> properties = null)
        {
            Geometry = geometry;
            Properties = properties ?? new Dictionary<string, object>();
        }

        public IGeometry Geometry { get; set; }
        public Dictionary<string, object> Properties { get; set; }
        public object Id { get; set; }

        public string ToGeoJson()
        {
            return new GeoJsonWriter().Write(this);
        }
    }
}
