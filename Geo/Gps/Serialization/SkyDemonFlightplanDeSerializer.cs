using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Linq;
using Geo.Gps.Serialization.Xml;

namespace Geo.Gps.Serialization;

public class SkyDemonFlightplanDeSerializer : GpsXmlDeSerializer
{
    // "N514807.00 W0000930.00": a hemisphere letter, then degrees, minutes and seconds run
    // together.
    //
    // The decimal point has to be a literal one. Written as a bare '.' it stood for any
    // character, so "N514807,00" matched and its seconds came through as "07,00" - which
    // double.Parse, allowing thousands separators by default, reads as seven hundred. That
    // placed the position 21 km north and 92 km west of where the file put it, and reported
    // nothing wrong. The decimals are optional, because a whole number of seconds is no less
    // valid and was rejected outright.
    private const string Seconds = @"(?<s>\d\d(?:\.\d+)?)";

    private const string LatitudePattern = @"^(?<dir>[NnSs])(?<d>\d\d)(?<m>\d\d)" + Seconds + "$";

    private const string LongitudePattern =
        @"^(?<dir>[EeWw])(?<d>\d\d\d)(?<m>\d\d)" + Seconds + "$";

    public override GpsFileFormat[] FileFormats
    {
        get { return new[] { new GpsFileFormat("flightplan", "SkyDemon Flightplan") }; }
    }

    public override GpsFeatures SupportedFeatures => GpsFeatures.Routes;

    /// <summary>
    /// The route as a list of waypoints, or <c>null</c> when any of its coordinates cannot
    /// be read.
    /// </summary>
    private static Route? ConvertRoute(XElement route, XNamespace ns)
    {
        var start = ParseWaypoint(route.AttributeValue("Start"));
        if (start == null)
            return null;

        var result = new Route();
        result.Waypoints.Add(start);

        // A route may consist of nothing but its starting point, in which case the
        // element is simply absent.
        foreach (var rhumbLine in route.Elements(ns + "RhumbLineRoute"))
        {
            var waypoint = ParseWaypoint(rhumbLine.AttributeValue("To"));
            if (waypoint == null)
                return null;

            result.Waypoints.Add(waypoint);
        }

        return result;
    }

    /// <summary>
    /// The waypoint <paramref name="value" /> names, or <c>null</c> when it does not hold a
    /// latitude and a longitude that this format can express.
    /// </summary>
    /// <remarks>
    /// Every way of failing used to leave the deserializer as an exception: a coordinate the
    /// pattern did not match reached <c>double.Parse</c> as an empty string, one written
    /// without a space between its ordinates ran off the end of the split, and an absent
    /// attribute was dereferenced. None of those is something a caller of
    /// <see cref="GpsData.Parse" /> has reason to expect. A document whose coordinates cannot
    /// be read is now reported the way this deserializer already reports one it cannot parse
    /// at all - by returning <c>null</c>.
    /// </remarks>
    private static Waypoint? ParseWaypoint(string? value)
    {
        if (value == null)
            return null;

        // Split on whitespace and discard the empties, so a file separating its two
        // ordinates by more than one space is read, and one separating them by none is
        // rejected rather than indexed past the end.
        var ordinates = value.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        if (ordinates.Length != 2)
            return null;

        var latitude = Regex.Match(ordinates[0], LatitudePattern);
        var longitude = Regex.Match(ordinates[1], LongitudePattern);
        if (!latitude.Success || !longitude.Success)
            return null;

        try
        {
            return new Waypoint(ToDegrees(latitude, 'S'), ToDegrees(longitude, 'W'));
        }
        catch (ArgumentOutOfRangeException)
        {
            // Degrees and minutes the pattern accepts but a position cannot hold, such as a
            // latitude of 99 degrees. What is out of range is the document, not an argument
            // the caller passed, so it is not raised at them as though it were theirs.
            return null;
        }
    }

    /// <summary>
    /// One ordinate in degrees, negated when its hemisphere letter is
    /// <paramref name="negative" />.
    /// </summary>
    private static double ToDegrees(Match match, char negative)
    {
        var degrees =
            ParseOrdinatePart(match.Groups["d"].Value)
            + ParseOrdinatePart(match.Groups["m"].Value) / 60
            + ParseOrdinatePart(match.Groups["s"].Value) / 3600;

        // Each ordinate's pattern admits only its own pair of hemisphere letters, so naming
        // the negative one settles it. Asking whether both were "N or E" instead, as this
        // did, quietly made a latitude southern whenever the match had failed and there was
        // no letter to read.
        return char.ToUpperInvariant(match.Groups["dir"].Value[0]) == negative ? -degrees : degrees;
    }

    // NumberStyles.Float, rather than the default that also allows thousands separators -
    // the leniency that turned a comma standing in for the decimal point into a hundredfold
    // error. The pattern no longer admits one, and this keeps a later change to it from
    // bringing the misreading back.
    private static double ParseOrdinatePart(string value)
    {
        return double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
    }

    protected override bool CanDeSerialize(XmlReader xml)
    {
        return xml.Name == "DivelementsFlightPlanner";
    }

    protected override GpsData? DeSerialize(XElement root)
    {
        var ns = root.Name.Namespace;
        var data = new GpsData();

        var primaryRoute = root.Element(ns + "PrimaryRoute");
        if (primaryRoute != null)
        {
            var primary = ConvertRoute(primaryRoute, ns);
            if (primary == null)
                return null;

            data.Routes.Add(primary);
        }

        foreach (var route in root.Elements(ns + "Route"))
        {
            var converted = ConvertRoute(route, ns);
            if (converted == null)
                return null;

            data.Routes.Add(converted);
        }

        var aircraft = root.Elements(ns + "Aircraft").FirstOrDefault();
        if (aircraft != null)
        {
            data.Metadata.Attribute(
                x => x.Vehicle.Identifier,
                aircraft.AttributeValue("Registration")
            );
            data.Metadata.Attribute(x => x.Vehicle.Model, aircraft.AttributeValue("Type"));
        }

        return data;
    }
}
