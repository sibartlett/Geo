using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;

namespace Geo.Gps.Serialization;

public class Gpx10Serializer : GpsXmlSerializer
{
    private const string GpxNamespace = "http://www.topografix.com/GPX/1/0";

    public override GpsFileFormat[] FileFormats
    {
        get
        {
            return new[]
            {
                new GpsFileFormat("gpx", "GPX 1.0", "http://www.topografix.com/GPX/1/0/gpx.xsd"),
            };
        }
    }

    public override GpsFeatures SupportedFeatures => GpsFeatures.All;

    protected override bool CanDeSerialize(XmlReader xml)
    {
        if (xml.LocalName != "gpx")
            return false;
        if (xml.NamespaceURI == GpxNamespace)
            return true;
        // Missing namespace: only claim the document when it explicitly declares
        // GPX 1.0; otherwise defer to the 1.1 serializer.
        return string.IsNullOrEmpty(xml.NamespaceURI) && xml.GetAttribute("version") == "1.0";
    }

    protected override GpsData DeSerialize(XElement root)
    {
        // The namespace is taken from the document rather than assumed, so a file
        // whose root is missing its default xmlns - a common defect in real-world
        // exports - reads through the same lookups as a well-formed one. For those
        // documents this is XNamespace.None and every child is looked up unqualified.
        var ns = root.Name.Namespace;

        var data = new GpsData();
        ParseMetadata(root, ns, data);
        ParseRoute(root, ns, data);
        ParseTracks(root, ns, data);
        ParseWaypoints(root, ns, data);
        return data;
    }

    // GPX 1.0 carries its metadata as direct children of <gpx>, in the order the
    // schema sequences them, rather than in the <metadata> element 1.1 introduced.
    protected override XDocument SerializeInternal(GpsData data)
    {
        XNamespace ns = GpxNamespace;

        var root = new XElement(
            ns + "gpx",
            new XAttribute("version", "1.0"),
            GetMetadata(data, x => x.Software) is { } creator
                ? new XAttribute("creator", creator)
                : null,
            XmlExtensions.OptionalElement(ns + "name", GetMetadata(data, x => x.Name)),
            XmlExtensions.OptionalElement(ns + "desc", GetMetadata(data, x => x.Description)),
            XmlExtensions.OptionalElement(ns + "author", GetMetadata(data, x => x.Author.Name)),
            XmlExtensions.OptionalElement(ns + "email", GetMetadata(data, x => x.Author.Email)),
            XmlExtensions.OptionalElement(ns + "url", GetMetadata(data, x => x.Link)),
            XmlExtensions.OptionalElement(ns + "keywords", GetMetadata(data, x => x.Keywords)),
            SerializeWaypoints(data, ns),
            SerializeRoutes(data, ns),
            SerializeTracks(data, ns)
        );

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    private IEnumerable<XElement> SerializeWaypoints(GpsData data, XNamespace ns)
    {
        return data.Waypoints.Select(waypoint => SerializeWaypoint(waypoint, ns + "wpt"));
    }

    private IEnumerable<XElement> SerializeTracks(GpsData data, XNamespace ns)
    {
        foreach (var track in data.Tracks)
        {
            yield return new XElement(
                ns + "trk",
                XmlExtensions.OptionalElement(ns + "name", GetTrackMetadata(track, x => x.Name)),
                XmlExtensions.OptionalElement(ns + "cmt", GetTrackMetadata(track, x => x.Comment)),
                XmlExtensions.OptionalElement(
                    ns + "desc",
                    GetTrackMetadata(track, x => x.Description)
                ),
                track.Segments.Select(segment => new XElement(
                    ns + "trkseg",
                    segment.Waypoints.Select(waypoint => SerializeWaypoint(waypoint, ns + "trkpt"))
                ))
            );
        }
    }

    private IEnumerable<XElement> SerializeRoutes(GpsData data, XNamespace ns)
    {
        foreach (var route in data.Routes)
        {
            yield return new XElement(
                ns + "rte",
                XmlExtensions.OptionalElement(ns + "name", GetRouteMetadata(route, x => x.Name)),
                XmlExtensions.OptionalElement(ns + "cmt", GetRouteMetadata(route, x => x.Comment)),
                XmlExtensions.OptionalElement(
                    ns + "desc",
                    GetRouteMetadata(route, x => x.Description)
                ),
                route.Waypoints.Select(waypoint => SerializeWaypoint(waypoint, ns + "rtept"))
            );
        }
    }

    // Children are emitted in the order the GPX 1.0 schema sequences them.
    private static XElement SerializeWaypoint(Waypoint waypoint, XName name)
    {
        XNamespace ns = name.Namespace;

        return new XElement(
            name,
            new XAttribute("lat", XmlExtensions.ToString((decimal)waypoint.Coordinate.Latitude)),
            new XAttribute("lon", XmlExtensions.ToString((decimal)waypoint.Coordinate.Longitude)),
            waypoint.Coordinate.Is3D
                ? new XElement(
                    ns + "ele",
                    XmlExtensions.ToString((decimal)((Is3D)waypoint.Coordinate).Elevation)
                )
                : null,
            waypoint.TimeUtc.HasValue
                ? new XElement(ns + "time", XmlExtensions.ToString(waypoint.TimeUtc.Value))
                : null,
            XmlExtensions.OptionalElement(ns + "name", waypoint.Name),
            XmlExtensions.OptionalElement(ns + "cmt", waypoint.Comment),
            XmlExtensions.OptionalElement(ns + "desc", waypoint.Description)
        );
    }

    private static void ParseMetadata(XElement root, XNamespace ns, GpsData data)
    {
        data.Metadata.Attribute(x => x.Software, root.AttributeValue("creator"));
        data.Metadata.Attribute(x => x.Name, root.ElementValue(ns + "name"));
        data.Metadata.Attribute(x => x.Description, root.ElementValue(ns + "desc"));
        data.Metadata.Attribute(x => x.Keywords, root.ElementValue(ns + "keywords"));
        data.Metadata.Attribute(x => x.Link, root.ElementValue(ns + "url"));
        data.Metadata.Attribute(x => x.Author.Name, root.ElementValue(ns + "author"));
        data.Metadata.Attribute(x => x.Author.Email, root.ElementValue(ns + "email"));
    }

    private static void ParseTracks(XElement root, XNamespace ns, GpsData data)
    {
        foreach (var trkType in root.Elements(ns + "trk"))
        {
            var track = new Track();

            track.Metadata.Attribute(x => x.Name, trkType.ElementValue(ns + "name"));
            track.Metadata.Attribute(x => x.Description, trkType.ElementValue(ns + "desc"));
            track.Metadata.Attribute(x => x.Comment, trkType.ElementValue(ns + "cmt"));

            foreach (var trksegType in trkType.Elements(ns + "trkseg"))
            {
                var trkpt = trksegType.Elements(ns + "trkpt").ToList();

                // A segment with no <trkpt> carries nothing and is left out, as it
                // was when an absent element deserialized to a null array.
                if (trkpt.Count == 0)
                    continue;

                var segment = new TrackSegment();
                foreach (var wptType in trkpt)
                    segment.Waypoints.Add(ConvertWaypoint(wptType, ns));
                track.Segments.Add(segment);
            }

            data.Tracks.Add(track);
        }
    }

    private static void ParseRoute(XElement root, XNamespace ns, GpsData data)
    {
        foreach (var rteType in root.Elements(ns + "rte"))
        {
            var route = new Route();
            route.Metadata.Attribute(x => x.Name, rteType.ElementValue(ns + "name"));
            route.Metadata.Attribute(x => x.Description, rteType.ElementValue(ns + "desc"));
            route.Metadata.Attribute(x => x.Comment, rteType.ElementValue(ns + "cmt"));

            // <rtept> is optional in the GPX schema, so a route may carry only
            // metadata.
            foreach (var wptType in rteType.Elements(ns + "rtept"))
                route.Waypoints.Add(ConvertWaypoint(wptType, ns));
            data.Routes.Add(route);
        }
    }

    private static void ParseWaypoints(XElement root, XNamespace ns, GpsData data)
    {
        foreach (var wptType in root.Elements(ns + "wpt"))
            data.Waypoints.Add(ConvertWaypoint(wptType, ns));
    }

    private static Waypoint ConvertWaypoint(XElement wptType, XNamespace ns)
    {
        var latitude = (double)(wptType.DecimalAttribute("lat") ?? 0m);
        var longitude = (double)(wptType.DecimalAttribute("lon") ?? 0m);
        var elevation = wptType.DecimalElement(ns + "ele");

        var point = elevation.HasValue
            ? new Point(latitude, longitude, (double)elevation.Value)
            : new Point(latitude, longitude);

        return new Waypoint(
            point,
            wptType.DateTimeElement(ns + "time"),
            wptType.ElementValue(ns + "name"),
            wptType.ElementValue(ns + "cmt"),
            wptType.ElementValue(ns + "desc")
        );
    }
}
