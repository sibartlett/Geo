using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11
{
    [XmlType(Namespace = "http://www.topografix.com/GPX/1/1")]
    public class GpxCopyright
    {
        [XmlElement(DataType = "gYear")]
        public string year { get; set; }

        [XmlElement(DataType = "anyURI")]
        public string license { get; set; }

        [XmlAttribute]
        public string author { get; set; }
    }
}
