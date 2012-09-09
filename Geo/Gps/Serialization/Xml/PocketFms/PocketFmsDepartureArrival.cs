using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsDepartureArrival {
        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string StringIdent { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public decimal Latitude { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public decimal Longitude { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string FriendlyShortname { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string Fullname { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string Elevation { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsRNav RNAV1 { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsRNav RNAV2 { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public PocketFmsDetailedObjectInfo DetailedObjectInfo { get; set; }
    }
}