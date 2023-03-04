using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
[XmlRoot("PocketFMSFlightplan", Namespace = "", IsNullable = false)]
public class PocketFmsFlightplan
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeta META { get; set; }

    //[XmlElement(Form=XmlSchemaForm.Unqualified)]
    //public PocketFmsFuel FUEL { get; set; }

    //[XmlElement(Form=XmlSchemaForm.Unqualified)]
    //public PocketFmsSupplementaryInformation SUPPLEMENTARYINFORMATION { get; set; }

    [XmlElement("LIB", Form = XmlSchemaForm.Unqualified)]
    public PocketFmsLib[] LIB { get; set; }

    //[XmlElement(Form=XmlSchemaForm.Unqualified)]
    //public PocketFmsAircraft AIRCRAFT { get; set; }
}