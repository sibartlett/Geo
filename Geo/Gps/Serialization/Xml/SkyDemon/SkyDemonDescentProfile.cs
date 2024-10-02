using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon;

[XmlType(AnonymousType = true)]
public class SkyDemonDescentProfile
{
    [XmlAttribute]
    public string Fpm { get; set; }

    [XmlAttribute]
    public string IndicatedAirspeed { get; set; }

    [XmlAttribute]
    public string FuelBurn { get; set; }
}
