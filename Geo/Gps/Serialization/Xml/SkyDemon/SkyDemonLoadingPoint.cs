using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon
{
    [XmlType(AnonymousType=true)]
    [XmlRoot("LoadingPoint", Namespace="", IsNullable=false)]
    public class SkyDemonLoadingPoint {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string LeverArm { get; set; }

        [XmlAttribute]
        public string LeverArmLat { get; set; }

        [XmlAttribute]
        public string DefaultValue { get; set; }

        [XmlAttribute]
        public string Weight { get; set; }
    }
}