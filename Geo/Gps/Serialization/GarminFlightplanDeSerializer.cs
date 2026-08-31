using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Geo.Gps.Serialization.Xml;

namespace Geo.Gps.Serialization;

public class GarminFlightplanDeSerializer : GpsXmlDeSerializer
{
    private const string FlightplanNamespace = "http://www8.garmin.com/xmlschemas/FlightPlan/v1";

    public override GpsFileFormat[] FileFormats
    {
        get { return new[] { new GpsFileFormat("fpl", "Garmin Flightplan") }; }
    }

    public override GpsFeatures SupportedFeatures => GpsFeatures.Routes;

    protected override bool CanDeSerialize(XmlReader xml)
    {
        return xml.NamespaceURI == FlightplanNamespace;
    }

    protected override GpsData DeSerialize(XElement root)
    {
        var ns = root.Name.Namespace;

        // The waypoints are held in one table and referenced by identifier from each
        // route point, so the table is indexed once rather than scanned per point.
        // First wins on a duplicated identifier - Single() used to raise instead,
        // which is not something a caller of GpsData.Parse has reason to expect.
        var waypoints = root.Element(ns + "waypoint-table")
            .ElementsOrEmpty(ns + "waypoint")
            .Where(x => x.ElementValue(ns + "identifier") != null)
            .GroupBy(x => x.ElementValue(ns + "identifier")!)
            .ToDictionary(x => x.Key, x => x.First());

        var data = new GpsData();
        foreach (var route in root.Elements(ns + "route"))
        {
            var rte = new Route();
            rte.Metadata.Attribute(x => x.Name, route.ElementValue(ns + "route-name"));

            foreach (var point in route.Elements(ns + "route-point"))
            {
                var identifier = point.ElementValue(ns + "waypoint-identifier");
                if (identifier == null || !waypoints.TryGetValue(identifier, out var waypoint))
                    continue;

                var latitude = waypoint.DoubleElement(ns + "lat");
                var longitude = waypoint.DoubleElement(ns + "lon");
                if (latitude == null || longitude == null)
                    continue;

                rte.Waypoints.Add(new Waypoint(latitude.Value, longitude.Value));
            }

            data.Routes.Add(rte);
        }

        return data;
    }
}
