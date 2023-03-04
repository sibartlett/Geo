using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Garmin.Flightplan;

[XmlType(AnonymousType = true, Namespace = "http://www8.garmin.com/xmlschemas/FlightPlan/v1")]
public class GarminRoutePoint
{
    [XmlElement("waypoint-identifier")] public string waypointidentifier { get; set; }

    //[XmlElement("waypoint-type")]
    //public string waypointtype { get; set; }

    //[XmlElement("waypoint-country-code")]
    //public string waypointcountrycode { get; set; }
}