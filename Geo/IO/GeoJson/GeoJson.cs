using System.IO;
using Geo.Abstractions.Interfaces;

namespace Geo.IO.GeoJson;

public static class GeoJson
{
    private static readonly GeoJsonReader Reader = new();
    private static readonly GeoJsonWriter Writer = new();

    public static string Serialize(object obj)
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
