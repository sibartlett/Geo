using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon
{
    [XmlType(AnonymousType=true)]
    [XmlRoot("DivelementsFlightPlanner", Namespace="", IsNullable=false)]
    public class SkyDemonFlightplan {
        [XmlElement("Aircraft", typeof (SkyDemonAircraft), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SkyDemonAircraft[] Aircraft { get; set; }
        //[XmlElement("LoadingPoint", typeof(SkyDemonLoadingPoint))]
        //public SkyDemonLoadingPoint[] LoadingPoint { get; set; }
        [XmlElement("PrimaryRoute", typeof(SkyDemonRoute), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SkyDemonRoute PrimaryRoute { get; set; }
        [XmlElement("Route", typeof(SkyDemonRoute), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        public SkyDemonRoute[] Routes { get; set; }
        //[XmlElement("WaypointNameHints", typeof (SkyDemonWaypointNameHints), Form = System.Xml.Schema.XmlSchemaForm.Unqualified)]
        //public SkyDemonWaypointNameHints WaypointNameHints { get; set; }
    }
}
