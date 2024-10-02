using System.IO;

namespace Geo.Gps.Serialization;

public interface IGpsFileDeSerializer
{
    GpsFileFormat[] FileFormats { get; }
    GpsFeatures SupportedFeatures { get; }
    bool CanDeSerialize(StreamWrapper streamWrapper);
    GpsData DeSerialize(StreamWrapper streamWrapper);
}

public interface IGpsFileSerializer : IGpsFileDeSerializer
{
    string Serialize(GpsData data);
    void Serialize(GpsData data, Stream stream);
}
