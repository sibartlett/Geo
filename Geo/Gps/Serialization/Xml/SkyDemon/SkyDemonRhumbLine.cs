using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon
{
    [XmlType(AnonymousType=true)]
    public class SkyDemonRhumbLine {
        [XmlAttribute]
        public string To { get; set; }

        [XmlAttribute]
        public string Level { get; set; }

        [XmlAttribute]
        public string LevelChange { get; set; }
    }
}