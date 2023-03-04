using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11;

[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public class GpxLink
{
    public string text { get; set; }

    public string type { get; set; }

    [XmlAttribute(DataType = "anyURI")] public string href { get; set; }
}