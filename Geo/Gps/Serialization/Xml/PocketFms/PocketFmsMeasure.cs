using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsMeasure<T> : PocketFmsValue<T>
{
    [XmlAttribute]
    public string Unit { get; set; }
}
