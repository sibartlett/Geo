using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsWeightAndBalanceData {
        [XmlElement("WBDataRecord", Form=XmlSchemaForm.Unqualified)]
        public PocketFmsWeightAndBalanceDataRecord[] WBDataRecord { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsMeasure<decimal> PlaneTotalWeight { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsMeasure<decimal> PlaneTotalArm { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsMeasure<decimal> PlaneTotalMoment { get; set; }
    }
}