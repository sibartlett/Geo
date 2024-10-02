using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon;

[XmlType(AnonymousType = true)]
public class SkyDemonAircraft
{
    //[XmlElement("ClimbProfile", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //public SkyDemonClimbProfile[] ClimbProfile { get; set; }

    //[XmlElement("DescentProfile", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //public SkyDemonDescentProfile[] DescentProfile { get; set; }

    //[XmlArray(Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //[XmlArrayItem("CruiseProfile", typeof (SkyDemonCruiseProfile[]), Form = System.Xml.Schema.XmlSchemaForm.Unqualified, IsNullable = false)]
    //public SkyDemonCruiseProfile[][] CruiseProfiles { get; set; }

    //[XmlElement("LoadingPoints", Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
    //public SkyDemonLoadingPoints[] LoadingPoints { get; set; }

    //[XmlAttribute]
    //public string Name { get; set; }

    [XmlAttribute]
    public string Registration { get; set; }

    //[XmlAttribute]
    //public string FuelType { get; set; }

    //[XmlAttribute]
    //public string FuelMeasurementType { get; set; }

    //[XmlAttribute]
    //public string FuelMeasurementVolumeType { get; set; }

    //[XmlAttribute]
    //public string FuelMeasurementMassType { get; set; }

    //[XmlAttribute]
    //public string TaxiFuel { get; set; }

    //[XmlAttribute]
    //public string LandingFuel { get; set; }

    //[XmlAttribute]
    //public string MaxFuel { get; set; }

    //[XmlAttribute]
    //public string EmptyWeight { get; set; }

    //[XmlAttribute]
    //public string EmptyArmLon { get; set; }

    //[XmlAttribute]
    //public string EmptyArmLat { get; set; }

    [XmlAttribute]
    public string Type { get; set; }

    //[XmlAttribute]
    //public string ColourMarkings { get; set; }

    //[XmlAttribute]
    //public string ServiceCeiling { get; set; }

    //[XmlAttribute]
    //public string Equipment1 { get; set; }

    //[XmlAttribute]
    //public string Equipment2 { get; set; }

    //[XmlAttribute]
    //public string FuelContingency { get; set; }

    //[XmlAttribute]
    //public string HoldingMinutes { get; set; }

    //[XmlAttribute]
    //public string HourlyCost { get; set; }

    //[XmlAttribute]
    //public string HourlyCostIncludesFuel { get; set; }

    //[XmlAttribute]
    //public string GlideAirspeed { get; set; }

    //[XmlAttribute]
    //public string GlideRatio { get; set; }

    //[XmlAttribute]
    //public string Envelope { get; set; }
}
