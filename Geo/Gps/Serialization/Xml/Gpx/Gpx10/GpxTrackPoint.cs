using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx.Gpx10
{
    [XmlType(AnonymousType=true, Namespace="http://www.topografix.com/GPX/1/0")]
    public class GpxTrackPoint : GpxPoint
    {
        public decimal course { get; set; }

        [XmlIgnore]
        public bool courseSpecified { get; set; }

        public decimal speed { get; set; }

        [XmlIgnore]
        public bool speedSpecified { get; set; }
    }
}