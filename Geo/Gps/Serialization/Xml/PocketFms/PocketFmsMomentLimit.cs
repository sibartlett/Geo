using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsMomentLimit
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> YValue_Weight { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> XValue_Moment { get; set; }
}