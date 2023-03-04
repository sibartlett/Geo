using System;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.PocketFms;

[XmlType(AnonymousType = true)]
public class PocketFMSFlightplanTimeMeasure
{
    [XmlAttribute(DataType = "time")] public DateTime Value { get; set; }

    [XmlAttribute] public string Unit { get; set; }
}