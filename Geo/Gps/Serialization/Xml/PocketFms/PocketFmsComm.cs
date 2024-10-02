using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsComm
{
    [XmlAttribute]
    public string Sector { get; set; }

    [XmlAttribute]
    public string OprHrs { get; set; }

    [XmlAttribute]
    public decimal Freq5 { get; set; }

    [XmlAttribute]
    public decimal Freq4 { get; set; }

    [XmlAttribute]
    public decimal Freq3 { get; set; }

    [XmlAttribute]
    public decimal Freq2 { get; set; }

    [XmlAttribute]
    public decimal Freq1 { get; set; }

    [XmlAttribute]
    public string CommType { get; set; }

    [XmlAttribute]
    public string CommRemarks { get; set; }
}
