using System;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.GpxBase
{
    public abstract class GpxMetadataBase
    {
        public string name { get; set; }
        public string desc { get; set; }
        public DateTime time { get; set; }
        [XmlIgnore]
        public bool timeSpecified { get; set; }
        public string keywords { get; set; }
    }
}
