using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11;

[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public class GpxTrack : GpxRteTrkBase
{
    [XmlElement("link")]
    public GpxLink[] link { get; set; }

    public string type { get; set; }

    //public extensionsType extensions { get; set; }

    [XmlElement("trkseg")]
    public GpxTrackSegment[] trkseg { get; set; }
}
