using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11;

[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public class GpxMetadata : GpxMetadataBase
{
    public GpxPerson author { get; set; }
    public GpxCopyright copyright { get; set; }

    [XmlElement("link")]
    public GpxLink[] link { get; set; }

    public GpxBounds bounds { get; set; }

    //public extensionsType extensions { get; set; }
}
