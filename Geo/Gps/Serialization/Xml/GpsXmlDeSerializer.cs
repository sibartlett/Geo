#nullable enable
using System;
using System.Xml;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml;

public abstract class GpsXmlDeSerializer<T> : IGpsFileDeSerializer
{
    protected readonly XmlSerializer _xmlSerializer = new(typeof(T));

    public abstract GpsFileFormat[] FileFormats { get; }
    public abstract GpsFeatures SupportedFeatures { get; }

    // When non-null, elements read from documents that are missing a namespace
    // are treated as belonging to this namespace during deserialization. This
    // lets malformed files (e.g. GPX exports without the default xmlns) parse
    // against the namespace-qualified models. Formats that do not need this
    // (or want strict matching) leave it null.
    protected virtual string? Namespace => null;

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
        streamWrapper.Position = 0;
        T doc;

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
                var effectiveReader =
                    Namespace == null ? reader : new NamespaceCoercingXmlReader(reader, Namespace);
                doc = (T)_xmlSerializer.Deserialize(effectiveReader);
            }
        }
        // XmlSerializer.Deserialize wraps malformed/unexpected XML in an
        // InvalidOperationException (with an inner XmlException), so catch both to
        // signal "cannot parse this document" by returning null.
        catch (XmlException)
        {
            return null;
        }
        catch (InvalidOperationException)
        {
            return null;
        }

        return DeSerialize(doc);
    }

    protected abstract bool CanDeSerialize(XmlReader xml);
    protected abstract GpsData? DeSerialize(T xml);
}
