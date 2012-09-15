using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Garmin.Flightplan
{
    [XmlType(AnonymousType=true, Namespace="http://www8.garmin.com/xmlschemas/FlightPlan/v1")]
    public class GarminRoute {
        [XmlElement("route-name")]
        public string routename { get; set; }

        //[XmlElement("flight-plan-index")]
        //public string flightplanindex { get; set; }

        [XmlElement("route-point")]
        public GarminRoutePoint[] routepoint { get; set; }
    }
}