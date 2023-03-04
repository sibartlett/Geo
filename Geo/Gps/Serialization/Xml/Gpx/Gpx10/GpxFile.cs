using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx10;

[XmlType(AnonymousType = true, Namespace = "http://www.topografix.com/GPX/1/0")]
[XmlRoot("gpx", Namespace = "http://www.topografix.com/GPX/1/0", IsNullable = false)]
public class GpxFile : GpxMetadataBase
{
    public GpxFile()
    {
        version = "1.0";
    }

    public string author { get; set; }

    public string email { get; set; }

    [XmlElement(DataType = "anyURI")] public string url { get; set; }

    public string urlname { get; set; }

    public GpxBounds bounds { get; set; }

    [XmlElement("wpt")] public GpxPoint[] wpt { get; set; }

    [XmlElement("rte")] public GpxRoute[] rte { get; set; }

    [XmlElement("trk")] public GpxTrack[] trk { get; set; }

    //[XmlAnyElement]
    //public XmlElement[] Any { get; set; }

    [XmlAttribute] public string version { get; set; }

    [XmlAttribute] public string creator { get; set; }
}