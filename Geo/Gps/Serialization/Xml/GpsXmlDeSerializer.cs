using System;
using System.Xml;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml
{
    public abstract class GpsXmlDeSerializer<T> : IGpsFileDeSerializer
    {
        protected readonly XmlSerializer _xmlSerializer = new XmlSerializer(typeof (T));
        public abstract GpsFeatures SupportedFeatures { get; }
        public abstract string[] FileExtensions { get; }
        public abstract Uri FileFormatSpecificationUri { get; }

        public bool CanDeSerialize(StreamWrapper streamWrapper)
        {
            try
            {
                streamWrapper.Position = 0;
                using (var reader = XmlReader.Create(streamWrapper, new XmlReaderSettings { CloseInput = false }))
                    return _xmlSerializer.CanDeserialize(reader);
            }
            catch (XmlException)
            {
                return false;
            }
        }

        public GpsData DeSerialize(StreamWrapper streamWrapper)
        {
            streamWrapper.Position = 0;
            T doc;

            try
            {
                streamWrapper.Position = 0;
                using (var reader = XmlReader.Create(streamWrapper, new XmlReaderSettings { CloseInput = false }))
                    doc = (T)_xmlSerializer.Deserialize(reader);
            }
            catch (XmlException)
            {
                return null;
            }

            return DeSerialize(doc);
        }

        protected abstract GpsData DeSerialize(T xml);
    }
}