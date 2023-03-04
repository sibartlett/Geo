using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11;

[XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
public enum GpxFixType
{
    none,
    [XmlEnum("2d")] Item2d,
    [XmlEnum("3d")] Item3d,
    dgps,
    pps
}