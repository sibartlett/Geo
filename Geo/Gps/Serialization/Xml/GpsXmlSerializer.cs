#nullable enable
using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Geo.Gps.Metadata;

namespace Geo.Gps.Serialization.Xml;

public abstract class GpsXmlSerializer : GpsXmlDeSerializer, IGpsFileSerializer
{
    public void Serialize(GpsData data, Stream stream)
    {
        using (var writer = XmlWriter.Create(stream, CreateWriterSettings()))
        {
            SerializeInternal(data).Save(writer);
        }
    }

    public async Task SerializeAsync(
        GpsData data,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
#if NET8_0_OR_GREATER
        var settings = CreateWriterSettings();
        settings.Async = true;

        // Disposed asynchronously as well as written asynchronously, so that
        // nothing in this path touches the destination stream synchronously.
        var writer = XmlWriter.Create(stream, settings);
        await using (writer.ConfigureAwait(false))
        {
            await SerializeInternal(data)
                .SaveAsync(writer, cancellationToken)
                .ConfigureAwait(false);
            await writer.FlushAsync().ConfigureAwait(false);
        }
#else
        // XDocument has no asynchronous Save on netstandard2.0, so the document is
        // written to an in-memory buffer synchronously and only the copy to the
        // destination stream is performed asynchronously.
        using (var tempStream = new MemoryStream())
        {
            using (var writer = XmlWriter.Create(tempStream, CreateWriterSettings()))
            {
                SerializeInternal(data).Save(writer);
            }

            tempStream.Position = 0;
            await tempStream
                .CopyToAsync(stream, 16 * 1024, cancellationToken)
                .ConfigureAwait(false);
        }
#endif
    }

    public string Serialize(GpsData data)
    {
        var textWriter = new EncodingStringWriter(Encoding.UTF8);
        using (var writer = XmlWriter.Create(textWriter, CreateWriterSettings()))
        {
            SerializeInternal(data).Save(writer);
        }

        return textWriter.ToString();
    }

    // CloseOutput stays off so that a caller's stream outlives the writer, matching
    // how XmlSerializer left the destination open.
    private static XmlWriterSettings CreateWriterSettings()
    {
        return new XmlWriterSettings
        {
            Encoding = new UTF8Encoding(false),
            Indent = true,
            CloseOutput = false,
        };
    }

    protected abstract XDocument SerializeInternal(GpsData data);

    // Both GPX schemas declare creator use="required". It used to be left out when
    // the metadata said nothing about what produced the file, which made every such
    // document invalid against the schema it announces in its own root element.
    private const string DefaultCreator = "Geo";

    /// <summary>
    /// What to name as the document's creator: what the data says produced it, or this
    /// library when it says nothing.
    /// </summary>
    /// <remarks>
    /// A creator that was read from a file is kept rather than replaced. The attribute
    /// names the software the document came from, and that a file has since passed
    /// through Geo does not make Geo its origin - a round-trip should not quietly
    /// reassign authorship of somebody's track.
    /// </remarks>
    protected static string GetCreator(GpsData data)
    {
        return GetMetadata(data, x => x.Software) ?? DefaultCreator;
    }

    // The three accessors below answer null for an attribute that is unset or blank,
    // so a caller can pass the result straight into XElement's constructor - which
    // ignores null content - instead of testing it first.
    protected static string? GetMetadata(
        GpsData data,
        Func<GpsMetadata.MetadataKeys, string> attribute
    )
    {
        var value = data.Metadata.Attribute(attribute);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    protected static string? GetTrackMetadata(
        Track data,
        Func<TrackMetadata.MetadataKeys, string> attribute
    )
    {
        var value = data.Metadata.Attribute(attribute);
        return string.IsNullOrWhiteSpace(value) ? null : value;
    }

    protected static string? GetRouteMetadata(
        Route data,
        Func<RouteMetadata.MetadataKeys, string> attribute
    )
    {
        var value = data.Metadata.Attribute(attribute);
        return string.IsNullOrWhiteSpace(value) ? null : value;
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
