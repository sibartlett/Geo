using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon;

[XmlType(AnonymousType = true)]
public class SkyDemonClimbProfile
{
    [XmlAttribute]
    public string FpmSL { get; set; }

    [XmlAttribute]
    public string FpmSC { get; set; }

    [XmlAttribute]
    public string IndicatedAirspeed { get; set; }

    [XmlAttribute]
    public string FuelBurnSL { get; set; }

    [XmlAttribute]
    public string FuelBurnSC { get; set; }
}
