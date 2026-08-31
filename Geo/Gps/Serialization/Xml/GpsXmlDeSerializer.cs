using System.Xml;
using System.Xml.Linq;

namespace Geo.Gps.Serialization.Xml;

public abstract class GpsXmlDeSerializer : IGpsFileDeSerializer
{
    public abstract GpsFileFormat[] FileFormats { get; }
    public abstract GpsFeatures SupportedFeatures { get; }

    public bool CanDeSerialize(StreamWrapper streamWrapper)
    {
        try
        {
            streamWrapper.Position = 0;
            using (
                var reader = XmlReader.Create(
                    streamWrapper,
                    new XmlReaderSettings { CloseInput = false }
                )
            )
            {
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

    public GpsData? DeSerialize(StreamWrapper streamWrapper)
    {
        XDocument document;

        try
        {
            streamWrapper.Position = 0;
            using (
                var reader = XmlReader.Create(
                    streamWrapper,
                    new XmlReaderSettings { CloseInput = false }
                )
            )
            {
                document = XDocument.Load(reader);
            }
        }
        // A document that is not well-formed is not one this deserializer can read,
        // which is reported by returning null rather than by raising at the caller.
        catch (XmlException)
        {
            return null;
        }

        return document.Root == null ? null : DeSerialize(document.Root);
    }

    /// <summary>
    /// Whether this deserializer claims the document whose root element
    /// <paramref name="xml" /> is positioned on.
    /// </summary>
    /// <remarks>
    /// Given a streaming reader rather than the loaded document, so that deciding which
    /// of the registered deserializers owns a file costs only the root element - the
    /// whole list is asked in turn before any of them parses anything.
    /// </remarks>
    protected abstract bool CanDeSerialize(XmlReader xml);

    protected abstract GpsData? DeSerialize(XElement root);
}
