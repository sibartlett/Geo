using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11
{
    [XmlType(Namespace="http://www.topografix.com/GPX/1/1")]
    public class GpxEmail {
        [XmlAttribute]
        public string id { get; set; }

        [XmlAttribute]
        public string domain { get; set; }
    }
}