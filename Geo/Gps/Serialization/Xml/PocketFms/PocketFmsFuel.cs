using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsFuel {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> TotalTripFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> TaxiAndDepartureFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> ArrivalAndTaxiFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> AlternateApproachFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsReserveFuel ReserveFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsContingencyFuel ContingencyFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> LoadedFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> TotalFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> LongestAlternateFuel { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsMeasureSpecified<decimal> LongestAlternateDistance { get; set; }
    }
}