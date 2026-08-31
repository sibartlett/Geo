using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using Geo.Gps;
using Geo.Measure;

// Exercises the XML-backed GPS serializers from a natively compiled binary.
//
// The documents are embedded rather than read from reference/ so the published
// executable can be run from anywhere, including a CI step that has already left
// the repository root behind.
//
// Every check is a plain comparison that writes what failed and moves the exit
// code off zero - a test framework would defeat the point, since what is being
// verified is that this code path works with no runtime code generation
// available to it.
internal static class Program
{
    private static int _failures;

    private static int Main()
    {
        RoundTripGpx11();
        RoundTripGpx10();
        ParseGpx10Document();
        ParseGarminFlightplan();
        ParsePocketFmsFlightplan();
        ParseSkyDemonFlightplan();
        ConvertUnits();

        if (_failures > 0)
        {
            Console.Error.WriteLine($"AOT smoke test failed: {_failures} check(s).");
            return 1;
        }

        Console.WriteLine("AOT smoke test passed.");
        return 0;
    }

    private const string Gpx11 = """
        <?xml version="1.0" encoding="utf-8"?>
        <gpx version="1.1" creator="Geo.AotSmokeTest" xmlns="http://www.topografix.com/GPX/1/1"
             xmlns:gpx_style="http://www.topografix.com/GPX/gpx_style/0/2">
          <metadata>
            <name>Smoke test</name>
            <desc>A short track</desc>
            <author><name>Ada Lovelace</name><email id="ada" domain="example.com" /></author>
          </metadata>
          <wpt lat="53.4808" lon="-2.2426"><ele>38</ele><name>Manchester</name>
            <link href="https://example.com/one"><text>one</text></link>
            <link href="https://example.com/two"><text>two</text></link>
          </wpt>
          <rte><name>A route</name><rtept lat="51.5072" lon="-0.1276" /></rte>
          <trk>
            <name>A track</name>
            <extensions>
              <gpx_style:line><gpx_style:color>C00000</gpx_style:color></gpx_style:line>
            </extensions>
            <trkseg>
              <trkpt lat="53.4808" lon="-2.2426"><ele>38</ele><time>2024-05-01T09:00:00Z</time></trkpt>
              <trkpt lat="53.8008" lon="-1.5491"><ele>45</ele><time>2024-05-01T10:00:00Z</time></trkpt>
            </trkseg>
          </trk>
        </gpx>
        """;

    private const string Gpx10 = """
        <?xml version="1.0" encoding="utf-8"?>
        <gpx version="1.0" creator="Geo.AotSmokeTest" xmlns="http://www.topografix.com/GPX/1/0">
          <name>Smoke test 1.0</name>
          <author>Ada Lovelace</author>
          <wpt lat="53.4808" lon="-2.2426"><ele>38</ele><name>Manchester</name></wpt>
        </gpx>
        """;

    private const string GarminFlightplan = """
        <?xml version="1.0" encoding="utf-8"?>
        <flight-plan xmlns="http://www8.garmin.com/xmlschemas/FlightPlan/v1">
          <waypoint-table>
            <waypoint><identifier>EGLG</identifier><lat>51.801944</lat><lon>-0.158333</lon></waypoint>
            <waypoint><identifier>MISC2</identifier><lat>51.946125</lat><lon>-0.018417</lon></waypoint>
          </waypoint-table>
          <route>
            <route-name>EGLG to MISC2</route-name>
            <route-point><waypoint-identifier>EGLG</waypoint-identifier></route-point>
            <route-point><waypoint-identifier>MISC2</waypoint-identifier></route-point>
          </route>
        </flight-plan>
        """;

    private const string PocketFmsFlightplan = """
        <?xml version="1.0" encoding="utf-8"?>
        <PocketFMSFlightplan>
          <META>
            <AircraftIdentification>D-EABC</AircraftIdentification>
            <AircraftType>C172</AircraftType>
            <PilotInCommand>Ada Lovelace</PilotInCommand>
          </META>
          <LIB>
            <FromPoint><Latitude>51.158882</Latitude><Longitude>14.950277</Longitude></FromPoint>
            <ToPoint><Latitude>51.308882</Latitude><Longitude>15.150277</Longitude></ToPoint>
          </LIB>
        </PocketFMSFlightplan>
        """;

    // The coordinates go through the regular expressions in
    // SkyDemonFlightplanDeSerializer, which is the one piece of this that depends on
    // System.Text.RegularExpressions holding up under AOT.
    private const string SkyDemonFlightplan = """
        <?xml version="1.0" encoding="utf-8"?>
        <DivelementsFlightPlanner>
          <Aircraft Registration="G-ABCD" Type="C172" />
          <PrimaryRoute Start="N514807.00 W0000930.00">
            <RhumbLineRoute To="N515646.05 W0000106.30" />
            <RhumbLineRoute To="N520738.65 E0001357.80" />
          </PrimaryRoute>
        </DivelementsFlightPlanner>
        """;

    private static void RoundTripGpx11()
    {
        var data = Parse("GPX 1.1", Gpx11);
        if (data == null)
            return;

        Check("GPX 1.1 creator", "Geo.AotSmokeTest", data.Metadata.Attribute(x => x.Software));
        Check("GPX 1.1 name", "Smoke test", data.Metadata.Attribute(x => x.Name));
        Check(
            "GPX 1.1 author email",
            "ada@example.com",
            data.Metadata.Attribute(x => x.Author.Email)
        );
        Check("GPX 1.1 waypoints", 1, data.Waypoints.Count);
        Check("GPX 1.1 routes", 1, data.Routes.Count);
        Check("GPX 1.1 track points", 2, data.Tracks.Single().Segments.Single().Waypoints.Count);

        // Write it back out and read it again: this is the path that used to need a
        // runtime-generated serialization assembly.
        var written = data.ToGpx();
        var reparsed = Parse("GPX 1.1 round-trip", written);
        if (reparsed == null)
            return;

        Check("round-tripped waypoints", data.Waypoints.Count, reparsed.Waypoints.Count);
        Check("round-tripped routes", data.Routes.Count, reparsed.Routes.Count);
        Check(
            "round-tripped track points",
            data.Tracks.Single().Segments.Single().Waypoints.Count,
            reparsed.Tracks.Single().Segments.Single().Waypoints.Count
        );
        Check(
            "round-tripped time",
            data.Tracks.Single().Segments.Single().Waypoints[0].TimeUtc,
            reparsed.Tracks.Single().Segments.Single().Waypoints[0].TimeUtc
        );
        Check(
            "round-tripped author email",
            "ada@example.com",
            reparsed.Metadata.Attribute(x => x.Author.Email)
        );

        Check("round-tripped links", 2, reparsed.Waypoints[0].Links.Count);
        Check("round-tripped link text", "two", reparsed.Waypoints[0].Links[1].Text);

        // Extension content is carried through as XML, so the round-trip exercises
        // XElement construction and namespace handling as well as the reader.
        XNamespace style = "http://www.topografix.com/GPX/gpx_style/0/2";
        Check(
            "round-tripped track extension",
            "C00000",
            reparsed.Tracks.Single().Extensions.SingleOrDefault()?.Element(style + "color")?.Value
        );
    }

    private static void RoundTripGpx10()
    {
        var data = Parse("GPX 1.0", Gpx10);
        if (data == null)
            return;

        Check("GPX 1.0 author", "Ada Lovelace", data.Metadata.Attribute(x => x.Author.Name));

        var reparsed = Parse("GPX 1.0 round-trip", data.ToGpx(GpxVersion.Gpx10));
        if (reparsed == null)
            return;

        Check("GPX 1.0 round-tripped waypoints", 1, reparsed.Waypoints.Count);
        Check("GPX 1.0 round-tripped name", "Manchester", reparsed.Waypoints[0].Name);
    }

    private static void ParseGpx10Document()
    {
        var data = Parse("GPX 1.0 elevation", Gpx10);
        if (data == null)
            return;

        Check("GPX 1.0 latitude", 53.4808, data.Waypoints[0].Coordinate.Latitude);
    }

    private static void ParseGarminFlightplan()
    {
        var data = Parse("Garmin", GarminFlightplan);
        if (data == null)
            return;

        Check(
            "Garmin route name",
            "EGLG to MISC2",
            data.Routes.Single().Metadata.Attribute(x => x.Name)
        );
        Check("Garmin route points", 2, data.Routes.Single().Waypoints.Count);
    }

    private static void ParsePocketFmsFlightplan()
    {
        var data = Parse("PocketFMS", PocketFmsFlightplan);
        if (data == null)
            return;

        Check("PocketFMS route points", 2, data.Routes.Single().Waypoints.Count);
        Check("PocketFMS aircraft", "C172", data.Metadata.Attribute(x => x.Vehicle.Model));
    }

    private static void ParseSkyDemonFlightplan()
    {
        var data = Parse("SkyDemon", SkyDemonFlightplan);
        if (data == null)
            return;

        Check("SkyDemon route points", 3, data.Routes.Single().Waypoints.Count);
        Check(
            "SkyDemon registration",
            "G-ABCD",
            data.Metadata.Attribute(x => x.Vehicle.Identifier)
        );

        // N514807.00 is 51 + 48/60 + 07/3600 degrees.
        var expected = 51 + (48 / 60d) + (7 / 3600d);
        Check(
            "SkyDemon latitude",
            Math.Round(expected, 6),
            Math.Round(data.Routes.Single().Waypoints[0].Coordinate.Latitude, 6)
        );
    }

    // The unit conversions used to read their factors off enum fields by reflection,
    // which the trimmer was free to remove; they are checked here for the same reason
    // the serializers are.
    private static void ConvertUnits()
    {
        Check("kilometre factor", 1000d, DistanceUnit.Km.GetConversionFactor());
        Check("nautical mile factor", 1852d, DistanceUnit.Nm.GetConversionFactor());
        Check("square kilometre factor", 1000d * 1000d, AreaUnit.Km.GetConversionFactor());
        Check("knot factor", 1852d / 3600d, SpeedUnit.Knots.GetConversionFactor());
    }

    private static GpsData? Parse(string what, string document)
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(document));
        var data = GpsData.Parse(stream);
        if (data == null)
        {
            Fail($"{what}: no deserializer claimed the document, or it failed to parse.");
            return null;
        }

        return data;
    }

    private static void Check<T>(string what, T expected, T actual)
    {
        if (!Equals(expected, actual))
            Fail($"{what}: expected '{expected}', got '{actual}'.");
    }

    private static void Fail(string message)
    {
        Console.Error.WriteLine(message);
        _failures++;
    }
}
