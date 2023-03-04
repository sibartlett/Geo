using System.Xml;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml;

public abstract class GpsXmlDeSerializer<T> : IGpsFileDeSerializer
{
    protected readonly XmlSerializer _xmlSerializer = new(typeof(T));

    public abstract GpsFileFormat[] FileFormats { get; }
    public abstract GpsFeatures SupportedFeatures { get; }

    public bool CanDeSerialize(StreamWrapper streamWrapper)
    {
        try
        {
            streamWrapper.Position = 0;
            using (var reader = XmlReader.Create(streamWrapper, new XmlReaderSettings { CloseInput = false }))
            {
                if (_xmlSerializer.CanDeserialize(reader))
                    if (reader.MoveToContent() == XmlNodeType.Element)
                        return CanDeSerialize(reader);
            }

            return false;
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
            {
                doc = (T)_xmlSerializer.Deserialize(reader);
            }
        }
        catch (XmlException)
        {
            return null;
        }

        return DeSerialize(doc);
    }

    protected abstract bool CanDeSerialize(XmlReader xml);
    protected abstract GpsData DeSerialize(T xml);
}