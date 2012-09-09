using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsAirspace {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsAirspaceEntryExitPoint AirspaceEntryPoint { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsAirspaceEntryExitPoint AirspaceExitPoint { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceName { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceType { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceComm1 { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceComm2 { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceCommName { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceCommAuth { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceLowerAltFt { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceUpperAltFt { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceClass { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceClassRemarks { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceSUASIdentifier { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceRemarks { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceActivity { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceActiveFromDateTime { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public string AirspaceActiveUpToDateTime { get; set; }
    }
}