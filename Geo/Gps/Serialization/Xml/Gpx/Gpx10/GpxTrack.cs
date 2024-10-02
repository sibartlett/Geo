using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx10;

[XmlType(AnonymousType = true, Namespace = "http://www.topografix.com/GPX/1/0")]
public class GpxTrack : GpxRteTrkBase
{
    [XmlElement(DataType = "anyURI")]
    public string url { get; set; }

    public string urlname { get; set; }

    //[XmlAnyElement]
    //public XmlElement[] Any { get; set; }

    [XmlElement("trkseg")]
    public GpxTrackSegment[] trkseg { get; set; }
}
