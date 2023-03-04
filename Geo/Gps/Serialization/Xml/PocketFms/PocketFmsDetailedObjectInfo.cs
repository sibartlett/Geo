using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsDetailedObjectInfo
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsAirportData AirportData { get; set; }

    [XmlArray(Form = XmlSchemaForm.Unqualified)]
    [XmlArrayItem("COMM", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public PocketFmsComm[] Communications { get; set; }

    [XmlArray(Form = XmlSchemaForm.Unqualified)]
    [XmlArrayItem("Remark", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
    public PocketFmsRemark[] Remarks { get; set; }
}