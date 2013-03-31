using System.Xml;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11
{
    [XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
    public class GpxExtensions
    {
        [XmlAnyElement]
        public XmlElement[] Any { get; set; }
    }
}
