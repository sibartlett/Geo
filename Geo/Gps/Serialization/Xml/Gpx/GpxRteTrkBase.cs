using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.Gpx;

public abstract class GpxRteTrkBase
{
    public string name { get; set; }
    public string cmt { get; set; }
    public string desc { get; set; }
    public string src { get; set; }

    [XmlElement(DataType = "nonNegativeInteger")]
    public string number { get; set; }
}
