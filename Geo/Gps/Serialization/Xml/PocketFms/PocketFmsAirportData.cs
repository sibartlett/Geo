using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsAirportData
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string Cityname { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string type { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string agency { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string fuel { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string oil { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string HasPrecisionApproachYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string HasNonPrecisionApproachYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string HasVFRReportingPointsYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsValue<short> LongestHardRunway { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsValue<short> LongestSoftRunway { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string HasWaterRunwayYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string IsAbandonedYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string AllowedAircraftTypes { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string HasMetarTafYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string PPRYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string EmergencyUseOnlyYN { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string VisibleButDestroyedYN { get; set; }
}