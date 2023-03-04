using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFmsAircraft
{
    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public string AircraftDescription { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<short> SpeedVx { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<short> SpeedVy { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<short> SpeedCruise { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<short> SpeedHolding { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<short> SpeedApproach { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> FuelConsumptionVx { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> FuelConsumptionVy { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> FuelConsumptionCruise { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> FuelConsumptionHolding { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> FuelConsumptionApproach { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> AvarageRateOfDescend { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsMeasure<decimal> AvarageRateOfClimb { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsSuitableDefinitions SuitableDefinitions { get; set; }

    [XmlElement(Form = XmlSchemaForm.Unqualified)]
    public PocketFmsWeightAndBalance WeightAndBalance { get; set; }
}