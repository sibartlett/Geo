using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx10;

[XmlType(Namespace = "http://www.topografix.com/GPX/1/0")]
public enum GpxFixType
{
    none,

    [XmlEnum("2d")]
    Item2d,

    [XmlEnum("3d")]
    Item3d,
    dgps,
    pps,
}
