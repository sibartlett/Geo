using System;
using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsMeta {
        //[XmlElement(Form=XmlSchemaForm.Unqualified, DataType="date")]
        //public DateTime TakeOffDateUTC { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string DOF { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified, DataType="time")]
        //public DateTime TakeOffTimeUTC { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<string> ICAOETD { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<string> ICAOTOTALEET { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<string> TOTALEET { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public PocketFmsDepartureArrival Departure { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public PocketFmsDepartureArrival Arrival { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string AircraftIdentification { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string RoutingString { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string FlightRules { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string FlightType { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string AircraftType { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string WakeTurbulence { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string Equipment { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string Transponder { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string AircraftColorAndMarkings { get; set; }

        [XmlElement(Form=XmlSchemaForm.Unqualified)]
        public string PilotInCommand { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string PilotInCommandCompany { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string PilotInCommandMobilePhone { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string PilotInCommandPhone { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string PilotInCommandFax { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string FlightRemarks { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string FlightPlanOtherInformation { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<string> ICAOEndurance { get; set; }

        //[XmlElement(Form=XmlSchemaForm.Unqualified)]
        //public string ICAOCruiseAltitude { get; set; }
    }
}