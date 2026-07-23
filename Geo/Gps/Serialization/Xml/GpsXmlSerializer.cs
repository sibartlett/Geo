using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geo.Gps.Metadata;

namespace Geo.Gps.Serialization.Xml;

public abstract class GpsXmlSerializer<T> : GpsXmlDeSerializer<T>, IGpsFileSerializer
{
    public void Serialize(GpsData data, Stream stream)
    {
        _xmlSerializer.Serialize(stream, SerializeInternal(data));
    }

    // XmlSerializer has no asynchronous Serialize, so the document is written to
    // an in-memory buffer synchronously and only the copy to the destination
    // stream is performed asynchronously.
    public async Task SerializeAsync(
        GpsData data,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        using (var tempStream = new MemoryStream())
        {
            _xmlSerializer.Serialize(tempStream, SerializeInternal(data));
            tempStream.Position = 0;
            await tempStream
                .CopyToAsync(stream, 16 * 1024, cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public string Serialize(GpsData data)
    {
        var textWriter = new EncodingStringWriter(Encoding.UTF8);
        _xmlSerializer.Serialize(textWriter, SerializeInternal(data));
        return textWriter.ToString();
    }

    protected abstract T SerializeInternal(GpsData data);

    protected void SerializeMetadata(
        GpsData data,
        T xml,
        Func<GpsMetadata.MetadataKeys, string> attribute,
        Action<T, string> action
    )
    {
        var value = data.Metadata.Attribute(attribute);
        if (!string.IsNullOrWhiteSpace(value))
            action(xml, value);
    }

    protected void SerializeTrackMetadata<TTrack>(
        Track data,
        TTrack xml,
        Func<TrackMetadata.MetadataKeys, string> attribute,
        Action<TTrack, string> action
    )
    {
        var value = data.Metadata.Attribute(attribute);
        if (!string.IsNullOrWhiteSpace(value))
            action(xml, value);
    }

    protected void SerializeRouteMetadata<TRoute>(
        Route data,
        TRoute xml,
        Func<RouteMetadata.MetadataKeys, string> attribute,
        Action<TRoute, string> action
    )
    {
        var value = data.Metadata.Attribute(attribute);
        if (!string.IsNullOrWhiteSpace(value))
            action(xml, value);
    }

    private class EncodingStringWriter : StringWriter
    {
        public EncodingStringWriter(Encoding encoding)
        {
            Encoding = encoding;
        }

        public override Encoding Encoding { get; }
    }
}
