using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsWeightAndBalance
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsWeightAndBalanceData WBData { get; set; }

    [XmlArray(Form = XmlSchemaForm.Unqualified)]
    [XmlArrayItem("WBMomentLimitPoint", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public PocketFmsMomentLimit[] WBMomentLimits { get; set; }

    [XmlArray(Form = XmlSchemaForm.Unqualified)]
    [XmlArrayItem("WBMomentLimitPoint", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public PocketFmsCgLimit[] WBCGLimits { get; set; }
}
