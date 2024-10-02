using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Garmin.Flightplan;

[XmlType(AnonymousType = true, Namespace = "http://www8.garmin.com/xmlschemas/FlightPlan/v1")]
public class GarminWaypoint
{
    public string identifier { get; set; }

    //public string type { get; set; }

    //[XmlElement("country-code")]
    //public string countrycode { get; set; }

    public double lat { get; set; }

    public double lon { get; set; }

    //public string comment { get; set; }
}
