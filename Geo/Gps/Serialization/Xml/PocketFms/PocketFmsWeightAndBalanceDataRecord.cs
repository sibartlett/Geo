using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsWeightAndBalanceDataRecord
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> Weight { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> Arm { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> Moment { get; set; }

    [XmlAttribute] public string WBDescription { get; set; }
}