using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx11
{
    [XmlType(Namespace="http://www.topografix.com/GPX/1/1")]
    public class GpxWaypoint : GpxWaypointBase
    {
        [XmlElement("link")]
        public GpxLink[] link { get; set; }

        public GpxFixType fix { get; set; }
        //public extensionsType extensions { get; set; }
    }
}