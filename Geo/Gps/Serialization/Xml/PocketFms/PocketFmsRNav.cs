using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsRNav
{
    [XmlAttribute] public decimal BEACONRADIAL { get; set; }

    [XmlAttribute] public string BEACONIDENT { get; set; }

    [XmlAttribute] public decimal BEACONFREQUENCY { get; set; }

    [XmlAttribute] public decimal BEACONDISTANCENM { get; set; }
}