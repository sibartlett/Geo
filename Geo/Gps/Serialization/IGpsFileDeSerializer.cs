using System;
using System.IO;

namespace Geo.Gps.Serialization
{
    public interface IGpsFileDeSerializer
    {
        string[] FileExtensions { get; }
        Uri FileFormatSpecificationUri { get; }
        bool CanDeSerialize(Stream stream);
        GpsData DeSerialize(Stream stream);
    }

    public interface IGpsFileSerializer : IGpsFileDeSerializer
    {
        void Serialize(Stream stream, GpsData data);
        string SerializeToString(GpsData data);
    }
}