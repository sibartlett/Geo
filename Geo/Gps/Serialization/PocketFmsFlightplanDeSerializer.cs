using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Geo.Gps.Serialization.Xml;

namespace Geo.Gps.Serialization;

public class PocketFmsFlightplanDeSerializer : GpsXmlDeSerializer
{
    public override GpsFileFormat[] FileFormats
    {
        get
        {
            return new[]
            {
                new GpsFileFormat(
                    "xml",
                    "PocketFMS Flightplan",
                    "http://www.PocketFMS.com/XMLSchema/PocketFMSNavlog-1.2.0.xsd"
                ),
            };
        }
    }

    public override GpsFeatures SupportedFeatures => GpsFeatures.Routes;

    protected override bool CanDeSerialize(XmlReader xml)
    {
        return xml.Name == "PocketFMSFlightplan";
    }

    /// <summary>
    /// The flightplan's single route and its aircraft details, or <c>null</c> when the
    /// document holds no leg this format can read.
    /// </summary>
    /// <remarks>
    /// A document without any &lt;LIB&gt; used to be indexed at [0] regardless, and its
    /// absent &lt;META&gt; dereferenced, so a file this deserializer had already claimed
    /// left it as an exception rather than as the <c>null</c> it reports for one it
    /// cannot parse at all.
    /// </remarks>
    protected override GpsData? DeSerialize(XElement root)
    {
        var ns = root.Name.Namespace;

        var legs = root.Elements(ns + "LIB").ToList();
        if (legs.Count == 0)
            return null;

        var start = ReadWaypoint(legs[0].Element(ns + "FromPoint"), ns);
        if (start == null)
            return null;

        var route = new Route();
        route.Waypoints.Add(start);

        foreach (var leg in legs)
        {
            var waypoint = ReadWaypoint(leg.Element(ns + "ToPoint"), ns);
            if (waypoint == null)
                return null;

            route.Waypoints.Add(waypoint);
        }

        var data = new GpsData();
        data.Routes.Add(route);

        var meta = root.Element(ns + "META");
        data.Metadata.Attribute(x => x.Vehicle.Crew1, meta.ElementValue(ns + "PilotInCommand"));
        data.Metadata.Attribute(
            x => x.Vehicle.Identifier,
            meta.ElementValue(ns + "AircraftIdentification")
        );
        data.Metadata.Attribute(x => x.Vehicle.Model, meta.ElementValue(ns + "AircraftType"));

        return data;
    }

    private static Waypoint? ReadWaypoint(XElement? point, XNamespace ns)
    {
        var latitude = point.DecimalElement(ns + "Latitude");
        var longitude = point.DecimalElement(ns + "Longitude");

        return latitude == null || longitude == null
            ? null
            : new Waypoint((double)latitude.Value, (double)longitude.Value);
    }
}
