using System;
using System.IO;
using System.Xml;
using System.Xml.Serialization;
using Geo.Gps.Metadata;

namespace Geo.Gps.Serialization.Xml
{
    public abstract class GpsXmlSerializer<T> : IGpsFileSerializer
    {
        private readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof (T));
        public abstract string[] FileExtensions { get; }
        public abstract Uri FileFormatSpecificationUri { get; }

        public bool CanDeSerialize(Stream stream)
        {
            stream.Position = 0;
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings {CloseInput = false}))
                return _xmlSerializer.CanDeserialize(reader);
        }

        public GpsData DeSerialize(Stream stream)
        {
            stream.Position = 0;
            T doc;
            using (var reader = XmlReader.Create(stream, new XmlReaderSettings { CloseInput = false }))
                doc = (T)_xmlSerializer.Deserialize(reader);

            return DeSerialize(doc);
        }

        public void Serialize(Stream stream, GpsData data)
        {
            _xmlSerializer.Serialize(stream, Serialize(data));
        }

        public string SerializeToString(GpsData data)
        {
            var textWriter = new StringWriter();
            _xmlSerializer.Serialize(textWriter, Serialize(data));
            return textWriter.ToString();
        }

        protected abstract GpsData DeSerialize(T xml);
        protected abstract T Serialize(GpsData data);

        protected void SerializeMetadata(GpsData data, T xml, Func<GpsMetadata.MetadataKeys, string> attribute, Action<T, string> action)
        {
            var value = data.Metadata.Attribute(attribute);
            if (!string.IsNullOrWhiteSpace(value))
                action(xml, value);
        }

        protected void SerializeTrackMetadata<TTrack>(Track data, TTrack xml, Func<TrackMetadata.MetadataKeys, string> attribute, Action<TTrack, string> action)
        {
            var value = data.Metadata.Attribute(attribute);
            if (!string.IsNullOrWhiteSpace(value))
                action(xml, value);
        }

        protected void SerializeRouteMetadata<TRoute>(Route data, TRoute xml, Func<RouteMetadata.MetadataKeys, string> attribute, Action<TRoute, string> action)
        {
            var value = data.Metadata.Attribute(attribute);
            if (!string.IsNullOrWhiteSpace(value))
                action(xml, value);
        }
    }
}
