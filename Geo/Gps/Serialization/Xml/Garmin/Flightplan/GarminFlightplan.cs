using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Garmin.Flightplan
{
    [XmlType(AnonymousType=true, Namespace="http://www8.garmin.com/xmlschemas/FlightPlan/v1")]
    [XmlRoot("flight-plan", Namespace="http://www8.garmin.com/xmlschemas/FlightPlan/v1", IsNullable=false)]
    public class GarminFlightplan {
        //public string created { get; set; }

        [XmlArray("waypoint-table")]
        [XmlArrayItem("waypoint", typeof (GarminWaypoint), IsNullable = false)]
        public GarminWaypoint[] waypointtable { get; set; }

        [XmlElement("route")]
        public GarminRoute[] route { get; set; }
    }
}
