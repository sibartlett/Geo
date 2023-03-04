using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;
using Geo.Gps.Serialization.Xml.Gpx.Gpx10;

namespace Geo.Gps.Serialization;

public class Gpx10Serializer : GpsXmlSerializer<GpxFile>
{
    public override GpsFileFormat[] FileFormats
    {
        get
        {
            return new[]
            {
                new GpsFileFormat("gps", "GPX 1.0", "http://www.topografix.com/GPX/1/0/gpx.xsd")
            };
        }
    }

    public override GpsFeatures SupportedFeatures => GpsFeatures.All;

    protected override bool CanDeSerialize(XmlReader xml)
    {
        return xml.NamespaceURI == "http://www.topografix.com/GPX/1/0";
    }

    protected override GpsData DeSerialize(GpxFile xml)
    {
        var data = new GpsData();
        ParseMetadata(xml, data);
        ParseRoute(xml, data);
        ParseTracks(xml, data);
        ParseWaypoints(xml, data);
        return data;
    }

    protected override GpxFile SerializeInternal(GpsData data)
    {
        var xml = new GpxFile();
        SerializeMetadata(data, xml, x => x.Software, (gpx, s) => gpx.creator = s);
        SerializeMetadata(data, xml, x => x.Name, (gpx, s) => gpx.name = s);
        SerializeMetadata(data, xml, x => x.Description, (gpx, s) => gpx.desc = s);
        SerializeMetadata(data, xml, x => x.Keywords, (gpx, s) => gpx.keywords = s);
        SerializeMetadata(data, xml, x => x.Link, (gpx, s) => gpx.url = s);
        SerializeMetadata(data, xml, x => x.Author.Name, (gpx, s) => gpx.author = s);
        SerializeMetadata(data, xml, x => x.Author.Email, (gpx, s) => gpx.email = s);

        xml.trk = SerializeTracks(data).ToArray();
        xml.rte = SerializeRoutes(data).ToArray();
        xml.wpt = SerializeWaypoints(data).ToArray();

        return xml;
    }

    private IEnumerable<GpxPoint> SerializeWaypoints(GpsData data)
    {
        return data.Waypoints.Select(waypoint => ConvertToGpxPoint(waypoint));
    }

    private IEnumerable<GpxTrack> SerializeTracks(GpsData data)
    {
        foreach (var track in data.Tracks)
        {
            var trk = new GpxTrack();

            SerializeTrackMetadata(track, trk, x => x.Name, (gpx, s) => gpx.name = s);
            SerializeTrackMetadata(track, trk, x => x.Description, (gpx, s) => gpx.desc = s);
            SerializeTrackMetadata(track, trk, x => x.Comment, (gpx, s) => gpx.cmt = s);

            trk.trkseg = new GpxTrackSegment[track.Segments.Count];
            for (var i = 0; i < track.Segments.Count; i++)
            {
                var segment = track.Segments[i];
                var pts = new GpxTrackPoint[segment.Waypoints.Count];
                for (var j = 0; j < segment.Waypoints.Count; j++) pts[j] = ConvertToGpxTrackPoint(segment.Waypoints[j]);
                trk.trkseg[i] = new GpxTrackSegment { trkpt = pts };
            }

            yield return trk;
        }
    }

    private IEnumerable<GpxRoute> SerializeRoutes(GpsData data)
    {
        foreach (var route in data.Routes)
        {
            var rte = new GpxRoute();

            SerializeRouteMetadata(route, rte, x => x.Name, (gpx, s) => gpx.name = s);
            SerializeRouteMetadata(route, rte, x => x.Description, (gpx, s) => gpx.desc = s);
            SerializeRouteMetadata(route, rte, x => x.Comment, (gpx, s) => gpx.cmt = s);

            rte.rtept = new GpxPoint[route.Waypoints.Count];
            for (var j = 0; j < route.Waypoints.Count; j++) rte.rtept[j] = ConvertToGpxPoint(route.Waypoints[j]);
            yield return rte;
        }
    }


    private static void ParseMetadata(GpxFile xml, GpsData data)
    {
        data.Metadata.Attribute(x => x.Software, xml.creator);
        data.Metadata.Attribute(x => x.Name, xml.name);
        data.Metadata.Attribute(x => x.Description, xml.desc);
        data.Metadata.Attribute(x => x.Keywords, xml.keywords);
        data.Metadata.Attribute(x => x.Link, xml.url);
        data.Metadata.Attribute(x => x.Author.Name, xml.author);
        data.Metadata.Attribute(x => x.Author.Email, xml.email);
    }

    private static void ParseTracks(GpxFile xml, GpsData data)
    {
        if (xml.trk != null)
            foreach (var trkType in xml.trk)
            {
                var track = new Track();

                track.Metadata.Attribute(x => x.Name, trkType.name);
                track.Metadata.Attribute(x => x.Description, trkType.desc);
                track.Metadata.Attribute(x => x.Comment, trkType.cmt);

                if (trkType.trkseg != null)
                    foreach (var trksegTrkpt in trkType.trkseg.Where(seg => seg.trkpt != null))
                    {
                        var segment = new TrackSegment();
                        foreach (var wptType in trksegTrkpt.trkpt) segment.Waypoints.Add(ConvertWaypoint(wptType));
                        track.Segments.Add(segment);
                    }

                data.Tracks.Add(track);
            }
    }

    private static void ParseRoute(GpxFile xml, GpsData data)
    {
        if (xml.rte != null)
            foreach (var rteType in xml.rte)
            {
                var route = new Route();
                route.Metadata.Attribute(x => x.Name, rteType.name);
                route.Metadata.Attribute(x => x.Description, rteType.desc);
                route.Metadata.Attribute(x => x.Comment, rteType.cmt);

                foreach (var wptType in rteType.rtept) route.Waypoints.Add(ConvertWaypoint(wptType));
                data.Routes.Add(route);
            }
    }

    private static void ParseWaypoints(GpxFile xml, GpsData data)
    {
        if (xml.wpt != null)
            foreach (var wptType in xml.wpt)
                data.Waypoints.Add(ConvertWaypoint(wptType));
    }

    private static Waypoint ConvertWaypoint(GpxPoint wptType)
    {
        var point = wptType.eleSpecified
            ? new Point((double)wptType.lat, (double)wptType.lon, (double)wptType.ele)
            : new Point((double)wptType.lat, (double)wptType.lon);
        var time = wptType.timeSpecified ? wptType.time : (DateTime?)null;
        return new Waypoint(point, time, wptType.name, wptType.cmt, wptType.desc);
    }

    private static GpxTrackPoint ConvertToGpxTrackPoint(Waypoint waypoint)
    {
        return new GpxTrackPoint
        {
            lat = (decimal)waypoint.Coordinate.Latitude,
            lon = (decimal)waypoint.Coordinate.Longitude,
            ele = waypoint.Coordinate.Is3D ? (decimal)((Is3D)waypoint.Coordinate).Elevation : 0m,
            eleSpecified = waypoint.Coordinate.Is3D,
            time = waypoint.TimeUtc.HasValue ? waypoint.TimeUtc.Value : DateTime.MinValue,
            timeSpecified = waypoint.TimeUtc.HasValue,
            name = waypoint.Name,
            desc = waypoint.Description,
            cmt = waypoint.Comment
        };
    }

    private static GpxPoint ConvertToGpxPoint(Waypoint waypoint)
    {
        return new GpxPoint
        {
            lat = (decimal)waypoint.Coordinate.Latitude,
            lon = (decimal)waypoint.Coordinate.Longitude,
            ele = waypoint.Coordinate.Is3D ? (decimal)((Is3D)waypoint.Coordinate).Elevation : 0m,
            eleSpecified = waypoint.Coordinate.Is3D,
            time = waypoint.TimeUtc.HasValue ? waypoint.TimeUtc.Value : DateTime.MinValue,
            timeSpecified = waypoint.TimeUtc.HasValue,
            name = waypoint.Name,
            desc = waypoint.Description,
            cmt = waypoint.Comment
        };
    }
}