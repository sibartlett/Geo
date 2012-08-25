using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;
using Geo.Gps.Serialization.Xml.Gpx11;

namespace Geo.Gps.Serialization
{
    public class Gpx11Serializer : GpsXmlSerializer<gpxType>
    {
        public override string[] FileExtensions { get { return new[] { "gpx" }; } }

        public override Uri FileFormatSpecificationUri { get { return new Uri("http://www.topografix.com/GPX/1/1/gpx.xsd"); } }

        protected override GpsData DeSerialize(gpxType xml)
        {
            var data = new GpsData();
            ParseMetadata(xml, data);
            ParseRoute(xml, data);
            ParseTracks(xml, data);
            ParseWaypoints(xml, data);
            return data;
        }

        protected override gpxType Serialize(GpsData data)
        {
            var xml = new gpxType();
            SerializeMetadata(data, xml, x => x.Software, (gpx, s) => gpx.creator = s);
            SerializeMetadata(data, xml, x => x.Name, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                gpx.metadata.name = s;
            });
            SerializeMetadata(data, xml, x => x.Description, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                gpx.metadata.desc = s;
            });
            SerializeMetadata(data, xml, x => x.Keywords, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                gpx.metadata.keywords = s;
            });

            SerializeMetadata(data, xml, x => x.Link, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.link == null)
                    gpx.metadata.link = new linkType[1];
                gpx.metadata.link[0] = new linkType { href = s };
            });

            SerializeMetadata(data, xml, x => x.Copyright.Author, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.copyright == null)
                    gpx.metadata.copyright = new copyrightType();
                gpx.metadata.copyright.author = s;
            });
            SerializeMetadata(data, xml, x => x.Copyright.License, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.copyright == null)
                    gpx.metadata.copyright = new copyrightType();
                gpx.metadata.copyright.license = s;
            });
            SerializeMetadata(data, xml, x => x.Copyright.Year, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.copyright == null)
                    gpx.metadata.copyright = new copyrightType();
                gpx.metadata.copyright.year = s;
            });

            SerializeMetadata(data, xml, x => x.Author.Name, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.author == null)
                    gpx.metadata.author = new personType();
                gpx.metadata.author.name = s;
            });
            SerializeMetadata(data, xml, x => x.Author.Email, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.author == null)
                    gpx.metadata.author = new personType();
                var parts = s.Split(new[] {'@'});
                gpx.metadata.author.email = new emailType
                    {
                        id = parts[0],
                        domain = parts[1],
                    };
            });

            SerializeMetadata(data, xml, x => x.Author.Link, (gpx, s) =>
            {
                if (gpx.metadata == null)
                    gpx.metadata = new metadataType();
                if (gpx.metadata.author == null)
                    gpx.metadata.author = new personType();
                if (gpx.metadata.author.link == null)
                    gpx.metadata.author.link = new linkType();
                gpx.metadata.author.link.href = s;
            });

            xml.trk = SerializeTracks(data).ToArray();
            xml.rte = SerializeRoutes(data).ToArray();
            xml.wpt = SerializeWaypoints(data).ToArray();

            return xml;
        }

        private IEnumerable<wptType> SerializeWaypoints(GpsData data)
        {
            return data.Waypoints.Select(waypoint => new wptType
            {
                lat = (decimal)waypoint.Latitude,
                lon = (decimal)waypoint.Longitude,
                ele = waypoint.Elevation == null ? 0m : (decimal)waypoint.Elevation
            });
        }

        private IEnumerable<trkType> SerializeTracks(GpsData data)
        {
            foreach (var track in data.Tracks)
            {
                var trk = new trkType();

                SerializeTrackMetadata(track, trk, x => x.Name, (gpx, s) => gpx.name = s);
                SerializeTrackMetadata(track, trk, x => x.Description, (gpx, s) => gpx.desc = s);
                SerializeTrackMetadata(track, trk, x => x.Comment, (gpx, s) => gpx.cmt = s);

                trk.trkseg = new trksegType[track.Segments.Count];
                for (var i = 0; i < track.Segments.Count; i++)
                {
                    var segment = track.Segments[i];
                    var pts = new wptType[segment.Count];
                    for (var j = 0; j < segment.Count; j++)
                    {
                        pts[j] = new wptType
                        {
                            lat = (decimal)segment[j].Latitude,
                            lon = (decimal)segment[j].Longitude,
                            ele = segment[j].Elevation == null ? 0m : (decimal)segment[j].Elevation
                        };
                    }
                    trk.trkseg[i] = new trksegType { trkpt = pts };
                }

                yield return trk;
            }
        }

        private IEnumerable<rteType> SerializeRoutes(GpsData data)
        {
            foreach (var route in data.Routes)
            {
                var rte = new rteType();

                SerializeRouteMetadata(route, rte, x => x.Name, (gpx, s) => gpx.name = s);
                SerializeRouteMetadata(route, rte, x => x.Description, (gpx, s) => gpx.desc = s);
                SerializeRouteMetadata(route, rte, x => x.Comment, (gpx, s) => gpx.cmt = s);

                rte.rtept = new wptType[route.LineString.Count];
                for (var j = 0; j < route.LineString.Count; j++)
                {
                    rte.rtept[j] = new wptType
                    {
                        lat = (decimal)route.LineString[j].Latitude,
                        lon = (decimal)route.LineString[j].Longitude,
                        ele = route.LineString[j].Elevation == null ? 0m : (decimal)route.LineString[j].Elevation
                    };
                }
                yield return rte;
            }
        }

        private static void ParseMetadata(gpxType xml, GpsData data)
        {
            data.Metadata.Attribute(x => x.Software, xml.creator);
            if (xml.metadata != null)
            {
                data.Metadata.Attribute(x => x.Name, xml.metadata.name);
                data.Metadata.Attribute(x => x.Description, xml.metadata.desc);
                data.Metadata.Attribute(x => x.Keywords, xml.metadata.keywords);

                if (xml.metadata.link != null && xml.metadata.link.Length > 0)
                    data.Metadata.Attribute(x => x.Link, xml.metadata.link[0].href);

                if (xml.metadata.author !=null)
                {
                    data.Metadata.Attribute(x => x.Author.Name, xml.metadata.author.name);
                    if (xml.metadata.author.email != null)
                        data.Metadata.Attribute(x => x.Author.Email, xml.metadata.author.email.id + "@" + xml.metadata.author.email.domain);
                    if (xml.metadata.author.link != null)
                        data.Metadata.Attribute(x => x.Author.Link, xml.metadata.author.link.href);
                }

                if (xml.metadata.copyright !=null)
                {
                    data.Metadata.Attribute(x => x.Copyright.Author, xml.metadata.copyright.author);
                    data.Metadata.Attribute(x => x.Copyright.License, xml.metadata.copyright.license);
                    data.Metadata.Attribute(x => x.Copyright.Year, xml.metadata.copyright.year);
                }
            }
        }

        private static void ParseTracks(gpxType xml, GpsData data)
        {
            if (xml.trk != null)
                foreach (var trkType in xml.trk)
                {
                    var track = new Track();

                    track.Metadata.Attribute(x => x.Name, trkType.name);
                    track.Metadata.Attribute(x => x.Description, trkType.desc);
                    track.Metadata.Attribute(x => x.Comment, trkType.cmt);

                    foreach (var trksegType in trkType.trkseg)
                    {
                        var segment = new LineString<Fix>();
                        foreach (var wptType in trksegType.trkpt)
                        {
                            var fix = new Fix((double)wptType.lat, (double)wptType.lon, (double)wptType.ele, wptType.time);
                            segment.Add(fix);
                        }
                        track.Segments.Add(segment);
                    }
                    data.Tracks.Add(track);
                }
        }

        private static void ParseRoute(gpxType xml, GpsData data)
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

        private static void ParseWaypoints(gpxType xml, GpsData data)
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
