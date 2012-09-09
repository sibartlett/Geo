using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon
{
    [XmlType(AnonymousType=true)]
    public class SkyDemonCruiseProfile {
        [XmlAttribute]
        public string Name { get; set; }

        [XmlAttribute]
        public string FuelBurn { get; set; }

        [XmlAttribute]
        public string IndicatedAirspeed { get; set; }

        [XmlAttribute]
        public string Airspeed { get; set; }

        [XmlAttribute]
        public string AirspeedType { get; set; }
    }
}