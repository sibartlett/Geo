using System;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.GpxBase
{
    public abstract class GpxWaypointBase
    {
        public decimal ele { get; set; }

        [XmlIgnore]
        public bool eleSpecified { get; set; }

        public DateTime time { get; set; }

        [XmlIgnore]
        public bool timeSpecified { get; set; }

        public decimal magvar { get; set; }

        [XmlIgnore]
        public bool magvarSpecified { get; set; }

        public decimal geoidheight { get; set; }

        [XmlIgnore]
        public bool geoidheightSpecified { get; set; }

        public string name { get; set; }

        public string cmt { get; set; }

        public string desc { get; set; }

        public string src { get; set; }

        public string sym { get; set; }

        public string type { get; set; }

        [XmlIgnore]
        public bool fixSpecified { get; set; }

        [XmlElement(DataType="nonNegativeInteger")]
        public string sat { get; set; }

        public decimal hdop { get; set; }

        [XmlIgnore]
        public bool hdopSpecified { get; set; }

        public decimal vdop { get; set; }

        [XmlIgnore]
        public bool vdopSpecified { get; set; }

        public decimal pdop { get; set; }

        [XmlIgnore]
        public bool pdopSpecified { get; set; }

        public decimal ageofdgpsdata { get; set; }

        [XmlIgnore]
        public bool ageofdgpsdataSpecified { get; set; }

        [XmlElement(DataType="integer")]
        public string dgpsid { get; set; }

        [XmlAttribute]
        public decimal lat { get; set; }

        [XmlAttribute]
        public decimal lon { get; set; }
    }
}
