using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;

namespace Geo.Gps.Serialization;

public class Gpx11Serializer : GpsXmlSerializer
{
    private const string GpxNamespace = "http://www.topografix.com/GPX/1/1";

    public override GpsFileFormat[] FileFormats
    {
        get
        {
            return new[]
            {
                new GpsFileFormat("gpx", "GPX 1.1", "http://www.topografix.com/GPX/1/1/gpx.xsd"),
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
        // Missing namespace: fall back to the version attribute, defaulting to
        // 1.1 when it is absent so namespaceless files still parse.
        return string.IsNullOrEmpty(xml.NamespaceURI) && xml.GetAttribute("version") != "1.0";
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

    protected override XDocument SerializeInternal(GpsData data)
    {
        XNamespace ns = GpxNamespace;

        var root = new XElement(
            ns + "gpx",
            new XAttribute("version", "1.1"),
            GetMetadata(data, x => x.Software) is { } creator
                ? new XAttribute("creator", creator)
                : null,
            SerializeMetadata(data, ns),
            SerializeWaypoints(data, ns),
            SerializeRoutes(data, ns),
            SerializeTracks(data, ns)
        );

        return new XDocument(new XDeclaration("1.0", "utf-8", null), root);
    }

    // The <metadata> element, or null when none of the fields it can hold are set -
    // GPX 1.1 makes it optional, and an empty one carries nothing.
    private XElement? SerializeMetadata(GpsData data, XNamespace ns)
    {
        var author = SerializeAuthor(data, ns);
        var copyright = SerializeCopyright(data, ns);
        var link = GetMetadata(data, x => x.Link);

        var metadata = new XElement(
            ns + "metadata",
            XmlExtensions.OptionalElement(ns + "name", GetMetadata(data, x => x.Name)),
            XmlExtensions.OptionalElement(ns + "desc", GetMetadata(data, x => x.Description)),
            author,
            copyright,
            link == null ? null : new XElement(ns + "link", new XAttribute("href", link)),
            XmlExtensions.OptionalElement(ns + "keywords", GetMetadata(data, x => x.Keywords))
        );

        return metadata.HasElements ? metadata : null;
    }

    private XElement? SerializeAuthor(GpsData data, XNamespace ns)
    {
        var name = GetMetadata(data, x => x.Author.Name);
        var link = GetMetadata(data, x => x.Author.Link);
        var email = SerializeEmail(GetMetadata(data, x => x.Author.Email), ns);

        if (name == null && link == null && email == null)
            return null;

        return new XElement(
            ns + "author",
            XmlExtensions.OptionalElement(ns + "name", name),
            email,
            link == null ? null : new XElement(ns + "link", new XAttribute("href", link))
        );
    }

    // GPX 1.1 splits an address into an id and a domain, so one that has no '@' to
    // split on - or nothing on either side of it - cannot be written at all. Leave
    // the element out rather than indexing past the end of the split and taking the
    // whole serialization down with it.
    private static XElement? SerializeEmail(string? address, XNamespace ns)
    {
        if (address == null)
            return null;

        var at = address.IndexOf('@');
        if (at <= 0 || at == address.Length - 1)
            return null;

        return new XElement(
            ns + "email",
            new XAttribute("id", address.Substring(0, at)),
            new XAttribute("domain", address.Substring(at + 1))
        );
    }

    private XElement? SerializeCopyright(GpsData data, XNamespace ns)
    {
        var author = GetMetadata(data, x => x.Copyright.Author);
        var year = GetMetadata(data, x => x.Copyright.Year);
        var license = GetMetadata(data, x => x.Copyright.License);

        if (author == null && year == null && license == null)
            return null;

        return new XElement(
            ns + "copyright",
            author == null ? null : new XAttribute("author", author),
            XmlExtensions.OptionalElement(ns + "year", year),
            XmlExtensions.OptionalElement(ns + "license", license)
        );
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

    // Children are emitted in the order the GPX 1.1 schema sequences them.
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

        var metadata = root.Element(ns + "metadata");
        if (metadata == null)
            return;

        data.Metadata.Attribute(x => x.Name, metadata.ElementValue(ns + "name"));
        data.Metadata.Attribute(x => x.Description, metadata.ElementValue(ns + "desc"));
        data.Metadata.Attribute(x => x.Keywords, metadata.ElementValue(ns + "keywords"));

        var link = metadata.Elements(ns + "link").FirstOrDefault();
        if (link != null)
            data.Metadata.Attribute(x => x.Link, link.AttributeValue("href"));

        var author = metadata.Element(ns + "author");
        if (author != null)
        {
            data.Metadata.Attribute(x => x.Author.Name, author.ElementValue(ns + "name"));

            var email = author.Element(ns + "email");
            if (email != null)
                data.Metadata.Attribute(
                    x => x.Author.Email,
                    email.AttributeValue("id") + "@" + email.AttributeValue("domain")
                );

            var authorLink = author.Element(ns + "link");
            if (authorLink != null)
                data.Metadata.Attribute(x => x.Author.Link, authorLink.AttributeValue("href"));
        }

        var copyright = metadata.Element(ns + "copyright");
        if (copyright != null)
        {
            data.Metadata.Attribute(x => x.Copyright.Author, copyright.AttributeValue("author"));
            data.Metadata.Attribute(
                x => x.Copyright.License,
                copyright.ElementValue(ns + "license")
            );
            data.Metadata.Attribute(x => x.Copyright.Year, copyright.ElementValue(ns + "year"));
        }
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
