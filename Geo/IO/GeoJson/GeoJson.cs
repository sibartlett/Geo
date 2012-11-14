using System.IO;
using Geo.Interfaces;

namespace Geo.IO.GeoJson
{
    public static class GeoJson
    {
        private static readonly GeoJsonReader Reader = new GeoJsonReader();
        private static readonly GeoJsonWriter Writer = new GeoJsonWriter();

        public static string Serialize(IGeoJsonObject obj)
        {
            return Writer.Write(obj);
        }

        public static IGeoJsonObject DeSerialize(string json)
        {
            return Reader.Read(json);
        }

        public static IGeoJsonObject DeSerialize(Stream stream)
        {
            return Reader.Read(stream);
        }
    }
}
