using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Geo.Gps.Metadata;
using Geo.Gps.Serialization;

namespace Geo.Gps;

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
        Waypoints = new List<Waypoint>();
    }

    public GpsMetadata Metadata { get; }
    public List<Route> Routes { get; set; }
    public List<Track> Tracks { get; set; }
    public List<Waypoint> Waypoints { get; set; }

    /// <summary>
    /// The file-level foreign content carried by the GPX document, as it appeared -
    /// one entry per extension element, each holding its own namespace and children.
    /// </summary>
    /// <remarks>
    /// GPX leaves &lt;extensions&gt; deliberately open, so the elements are handed over
    /// as XML for the caller to read with LINQ to XML rather than modelled: no fixed
    /// set of properties could keep up with what Garmin, Gaia GPS, the Topografix
    /// gpx_style schema and the rest put in there. What the library guarantees is that
    /// nothing is lost - anything read is written back.
    /// <para>
    /// GPX 1.1 keeps this in the &lt;gpx&gt; element's &lt;extensions&gt;; 1.0 has no
    /// such element and holds the same content inline at the end of &lt;gpx&gt;. Both
    /// are read here, and each is written in its own version's shape. Content a 1.1
    /// document holds in &lt;metadata&gt;&lt;extensions&gt; is read here too, and
    /// written back at the &lt;gpx&gt; level, which is where 1.0 would carry it.
    /// </para>
    /// </remarks>
    public List<XElement> Extensions { get; } = new List<XElement>();

    /// <summary>
    /// The smallest envelope containing every coordinate this holds - waypoints, route
    /// points and track points alike - or <c>null</c> when there are none.
    /// </summary>
    /// <remarks>
    /// GPX defines its &lt;bounds&gt; element as the extent of the coordinates in the
    /// file, so the serializers compute it from the data at the point of writing rather
    /// than storing what they read. Kept, it would go stale the moment a caller added a
    /// waypoint, and the file would then carry an extent that did not describe it.
    /// </remarks>
    public Envelope? GetBounds()
    {
        var bounds = Waypoints.Aggregate(
            (Envelope?)null,
            // Point.GetBounds is null-safe where Waypoint.Coordinate is not: a waypoint
            // holding an empty Point has no coordinate, and this promises null for data
            // with none rather than faulting on it.
            (current, waypoint) => waypoint.Point.GetBounds()?.Combine(current) ?? current
        );

        // A route or track holding no waypoints has no bounds to fold in; Combine is an
        // instance method, so the null has to be stepped around rather than passed.
        bounds = Routes.Aggregate(
            bounds,
            (current, route) => route.GetBounds()?.Combine(current) ?? current
        );

        return Tracks.Aggregate(
            bounds,
            (current, track) => track.GetBounds()?.Combine(current) ?? current
        );
    }

    public string ToGpx()
    {
        return FileSerializers[1].Serialize(this);
    }

    public string ToGpx(decimal version)
    {
        var index = version == 1m ? 0 : 1;
        return FileSerializers[index].Serialize(this);
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

    public static GpsData? Parse(Stream stream)
    {
        var gpsStream = new StreamWrapper(stream);
        var parser = FileParsers.FirstOrDefault(x => x.CanDeSerialize(gpsStream));
        return parser == null ? null : parser.DeSerialize(gpsStream);
    }

    public static async Task<GpsData?> ParseAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        var gpsStream = await StreamWrapper
            .CreateAsync(stream, cancellationToken)
            .ConfigureAwait(false);
        var parser = FileParsers.FirstOrDefault(x => x.CanDeSerialize(gpsStream));
        return parser == null ? null : parser.DeSerialize(gpsStream);
    }
}
