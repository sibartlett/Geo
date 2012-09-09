using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms
{
    [XmlType(AnonymousType=true)]
    public class PocketFmsLib {
        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsPoint FromPoint { get; set; }

        [XmlElement(Form = XmlSchemaForm.Unqualified)]
        public PocketFmsPoint ToPoint { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified, DataType = "time")]
        //public System.DateTime ETAUTC { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> MSA { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> PlannedAltitude { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> OAT { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> IAS { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> QNH { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> TAS { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string ALTERNATEIDENT { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<decimal> ALTERNATEDISTANCE { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<decimal> FUEL { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> WINDDIRECTION { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> WINDSPEED { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public sbyte AVARAGEVARIATION { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFMSFlightplanTimeMeasure ETE { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFMSFlightplanTimeMeasure ETECUMMULATIVE { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> TRUETRACK { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> MAGNETICTRACK { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WINDCORRECTIONANGLE { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> TRUEHEADING { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> MAGNETICHEADING { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<short> GROUNDSPEED { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<decimal> DistanceCummulative { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public PocketFmsMeasure<decimal> Distance { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WXSignificant { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WXMETAR { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WXSHORTTAF { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WXLONGTAF { get; set; }

        //[XmlElement(Form = XmlSchemaForm.Unqualified)]
        //public string WXALLTAF { get; set; }

        //[XmlArray(Form = XmlSchemaForm.Unqualified)]
        //[XmlArrayItem("Airspace", Form = XmlSchemaForm.Unqualified, IsNullable = false)]
        //public PocketFmsAirspace[] AirspacesCrossed { get; set; }

        //[XmlAttribute]
        //public sbyte LibID { get; set; }
    }
}