using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsSuitableDefinitions
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte CivilianAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte MilitaryAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte JoinedAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte ULMAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte GliderAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte HeliAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte WaterAirports { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte MusthavePrecisionApproach { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte MusthaveNonPrecisionApproach { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> MinimumHardRunwayLength { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> MinimumSoftRunwayLength { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> MinimumCloudbase { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> MaximumCrosswind { get; set; }
}
