using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx10;

[XmlType(AnonymousType = true, Namespace = "http://www.topografix.com/GPX/1/0")]
public class GpxTrackSegment
{
    [XmlElement("trkpt")] public GpxTrackPoint[] trkpt { get; set; }
}