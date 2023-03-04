using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsSupplementaryInformation
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<string> Endurance { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte POB { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bRadio { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bRadioUHF { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bRadioVHF { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bRadioELBA { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bSurvivalEquipment { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bSurvivalPolar { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bSurvivalDesert { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bSurvivalMaritime { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bSurvivalJungle { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bJackets { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bJacketsLights { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bJacketsFluores { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bJacketsUHF { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bJacketsVHF { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bDinghies { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string DinghiesNumber { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string DinghiesCapacity { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public sbyte bDinghiesCover { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string DinghiesCoverColor { get; set; }
}