using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsContingencyFuel : PocketFmsMeasureSpecified<decimal>
    {
        [XmlAttribute]
        public short Percentage { get; set; }

        [XmlIgnore]
        public bool PercentageSpecified { get; set; }
    }
}