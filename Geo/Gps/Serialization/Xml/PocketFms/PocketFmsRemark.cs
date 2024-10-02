using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsRemark
{
    [XmlAttribute]
    public string RemarkType { get; set; }

    [XmlAttribute]
    public string RemarkText { get; set; }
}
