using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11
{
    [XmlType(Namespace="http://www.topografix.com/GPX/1/1")]
    [XmlRoot("gpx", Namespace="http://www.topografix.com/GPX/1/1", IsNullable=false)]
    public class GpxFile {
        public GpxFile() {
            this.version = "1.1";
        }

        public GpxMetadata metadata { get; set; }

        [XmlElement("wpt")]
        public GpxWaypoint[] wpt { get; set; }

        [XmlElement("rte")]
        public GpxRoute[] rte { get; set; }

        [XmlElement("trk")]
        public GpxTrack[] trk { get; set; }

        //public extensionsType extensions { get; set; }

        [XmlAttribute]
        public string version { get; set; }

        [XmlAttribute]
        public string creator { get; set; }
    }
}