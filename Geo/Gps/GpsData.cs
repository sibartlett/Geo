using System.Collections.Generic;
using System.IO;
using System.Linq;
using Geo.Geometries;
using Geo.Gps.Metadata;
using Geo.Gps.Serialization;

namespace Geo.Gps
{
    public class GpsData
    {
        private static readonly List<IGpsFileSerializer> FileSerializers;
        private static readonly List<IGpsFileDeSerializer> FileParsers;

        static GpsData()
        {
            FileSerializers = new List<IGpsFileSerializer>
                    {
                        new Gpx10Serializer(),
                        new Gpx11Serializer(),
                    };
            FileParsers = new List<IGpsFileDeSerializer>(FileSerializers.OfType<IGpsFileDeSerializer>())
                    {
                        new IgcDeSerializer(),
                        new NmeaDeSerializer(),
                        new GarminFlightplanDeSerializer(),
                        new PocketFmsFlightplanDeSerializer(),
                        new SkyDemonFlightplanDeSerializer(),
                    };
        }

        public GpsData()
        {
            Metadata = new GpsMetadata();
            Routes = new List<Route>();
            Tracks = new List<Track>();
            Waypoints = new List<Point>();
        }

        public GpsMetadata Metadata { get; private set; }
        public List<Route> Routes { get; set; }
        public List<Track> Tracks { get; set; }
        public List<Point> Waypoints { get; set; }

        public string ToGpx()
        {
            return FileSerializers[1].SerializeToString(this);
        }

        public string ToGpx(decimal version)
        {
            var index = version == 1m ? 0 : 1;
            return FileSerializers[index].SerializeToString(this);
        }

        public static IEnumerable<GpsFileFormat> SupportedGpsFileFormats()
        {
            return FileParsers
                .SelectMany(x => x.FileFormats)
                .OrderBy(x => x.Extension)
                .ThenBy(x => x.Name);
        }

        public static IEnumerable<GpsFileFormat> SupportedGpsFileFormats(GpsFeatures features)
        {
            return FileParsers
                .Where(x => x.SupportedFeatures.Contains(features))
                .SelectMany(x => x.FileFormats)
                .OrderBy(x => x.Extension)
                .ThenBy(x => x.Name);
        }

        public static GpsData Parse(Stream stream)
        {
            var gpsStream = new StreamWrapper(stream);
            var parser = FileParsers.FirstOrDefault(x => x.CanDeSerialize(gpsStream));
            return parser == null ? null : parser.DeSerialize(gpsStream);
        }
    }
}