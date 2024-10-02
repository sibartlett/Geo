using System.Xml.Schema;
using System.Xml.Serialization;

namespace Geo.Gps.Serialization.Xml.SkyDemon;

[XmlType(AnonymousType = true)]
public class SkyDemonWaypointNameHints
{
    [XmlElement("Hint", Form = XmlSchemaForm.Unqualified)]
    public SkyDemonWaypointNameHint[] Hint { get; set; }
}
