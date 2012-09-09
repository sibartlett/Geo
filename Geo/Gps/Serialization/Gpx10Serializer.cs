using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;
using Geo.Gps.Serialization.Xml.Gpx.Gpx10;

namespace Geo.Gps.Serialization
{
    public class Gpx10Serializer : GpsXmlSerializer<GpxFile>
    {
        public override string[] FileExtensions
        {
            get { return new[] {"gpx"}; }
        }

        public override Uri FileFormatSpecificationUri
        {
            get { return new Uri("http://www.topografix.com/GPX/1/0/gpx.xsd"); }
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

        protected override GpxFile Serialize(GpsData data)
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
            return data.Waypoints.Select(waypoint => new GpxPoint
                {
                    lat = (decimal)waypoint.Latitude,
                    lon = (decimal)waypoint.Longitude,
                    ele = waypoint.Elevation == null ? 0m : (decimal)waypoint.Elevation
                });
        }

        private IEnumerable<GpxTrack> SerializeTracks(GpsData data)
        {
            foreach (var track in data.Tracks)
            {
                var trk = new GpxTrack();

                SerializeTrackMetadata(track, trk, x => x.Name, (gpx, s) => gpx.name = s);
                SerializeTrackMetadata(track, trk, x => x.Description, (gpx, s) => gpx.desc = s);
                SerializeTrackMetadata(track, trk, x => x.Comment, (gpx, s) => gpx.cmt = s);

                trk.trkseg = new GpxTrackPoint[track.Segments.Count][];
                for (var i = 0; i < track.Segments.Count; i++)
                {
                    var segment = track.Segments[i];
                    var pts = new GpxTrackPoint[segment.Points.Count];
                    for (var j = 0; j < segment.Points.Count; j++)
                    {
                        pts[j] = new GpxTrackPoint
                        {
                            lat = (decimal)segment[j].Latitude,
                            lon = (decimal)segment[j].Longitude,
                            ele = segment[j].Elevation == null ? 0m : (decimal)segment[j].Elevation
                        };
                    }
                    trk.trkseg[i] = pts;
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

                rte.rtept = new GpxPoint[route.LineString.Points.Count];
                for (var j = 0; j < route.LineString.Points.Count; j++)
                {
                    rte.rtept[j] = new GpxPoint
                    {
                        lat = (decimal)route.LineString[j].Latitude,
                        lon = (decimal)route.LineString[j].Longitude,
                        ele = route.LineString[j].Elevation == null ? 0m : (decimal)route.LineString[j].Elevation
                    };
                }
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
                
                    foreach (var trksegTrkpt in trkType.trkseg)
                    {
                        var segment = new LineString<Fix>();
                        foreach (var wptType in trksegTrkpt)
                        {
                            var fix = new Fix((double)wptType.lat, (double)wptType.lon, (double)wptType.ele, wptType.time);
                            segment.Add(fix);
                        }
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

                    route.LineString = new LineString<Point>();
                    foreach (var wptType in rteType.rtept)
                    {
                        var fix = new Fix((double)wptType.lat, (double)wptType.lon, (double)wptType.ele, wptType.time);
                        route.LineString.Add(fix);
                    }
                    data.Routes.Add(route);
                }
        }

        private static void ParseWaypoints(GpxFile xml, GpsData data)
        {
            if (xml.wpt != null)
                foreach (var wptType in xml.wpt)
                {
                    var fix = new Fix((double)wptType.lat, (double)wptType.lon, (double)wptType.ele, wptType.time);
                    data.Waypoints.Add(fix);
                }
        }
    }
}
