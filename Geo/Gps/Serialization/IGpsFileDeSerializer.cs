using System.IO;

namespace Geo.Gps.Serialization
{
    public interface IGpsFileDeSerializer
    {
        GpsFileFormat[] FileFormats { get; }
        GpsFeatures SupportedFeatures { get; }
        bool CanDeSerialize(StreamWrapper streamWrapper);
        GpsData DeSerialize(StreamWrapper streamWrapper);
    }

    public interface IGpsFileSerializer : IGpsFileDeSerializer
    {
        void Serialize(Stream stream, GpsData data);
        string SerializeToString(GpsData data);
    }
}