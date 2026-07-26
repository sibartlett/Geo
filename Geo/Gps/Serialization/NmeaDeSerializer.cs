#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Geo.Geometries;

namespace Geo.Gps.Serialization;

public class NmeaDeSerializer : IGpsFileDeSerializer
{
    private const string WPT_SENTENCE =
        @"^\$GPWPL,(?<lat>(?:\d+\.?\d*|\d*\.?\d+)),(?<latd>[NnSs]),(?<lon>(?:\d+\.?\d*|\d*\.?\d+)),(?<lond>[EeWw]),(?<id>[\d\w]+)\*[\d\w][\d\w]";

    private const string FIX_SENTENCE =
        @"^\$GPGGA\,(?<h>\d\d)(?<m>\d\d)(?<s>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<lat>(?:\d+\.?\d*|\d*\.?\d+))\,(?<latd>[NnSs])\,(?<lon>(?:\d+\.?\d*|\d*\.?\d+))\,(?<lond>[EeWw])\,(?<qual>[012])\,(?<sat>\d*)\,(?<hdop>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<alt>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<altU>[Mm])\,(?<geoid>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<geoidU>[Mm])\,(?<last>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<stat>\d\d\d\d)\*[\d\w][\d\w]$";

    public GpsFileFormat[] FileFormats
    {
        get { return new[] { new GpsFileFormat("nmea", "NMEA") }; }
    }

    public GpsFeatures SupportedFeatures => GpsFeatures.TracksAndWaypoints;

    public bool CanDeSerialize(StreamWrapper streamWrapper)
    {
        streamWrapper.Position = 0;
        using (var reader = new StreamReader(streamWrapper))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
                if (Regex.IsMatch(line, FIX_SENTENCE) || Regex.IsMatch(line, WPT_SENTENCE))
                    return true;
        }

        return false;
    }

    public GpsData DeSerialize(StreamWrapper streamWrapper)
    {
        var data = new GpsData();
        var trackSegment = new TrackSegment();
        // Local to this call: the deserializers are held as shared singletons by GpsData,
        // so per-file state cannot live on the instance.
        var clock = new FixClock();
        streamWrapper.Position = 0;
        using (var reader = new StreamReader(streamWrapper))
        {
            string? line;
            while ((line = reader.ReadLine()) != null)
            {
                if (ParseFix(line, trackSegment, clock))
                    continue;
                if (ParseWaypoint(line, data))
                    continue;
            }
        }

        if (!trackSegment.IsEmpty())
        {
            data.Tracks.Add(new Track());
            data.Tracks[0].Segments.Add(trackSegment);
        }

        return data;
    }

    private bool ParseFix(string line, TrackSegment trackSegment, FixClock clock)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var match = Regex.Match(line, FIX_SENTENCE);
        if (match.Success)
        {
            var alt = double.Parse(match.Groups["alt"].Value, CultureInfo.InvariantCulture);
            if (!TryConvertPosition(match, alt, out var position))
                return false;

            var h = int.Parse(match.Groups["h"].Value, CultureInfo.InvariantCulture);
            var m = int.Parse(match.Groups["m"].Value, CultureInfo.InvariantCulture);
            var s = double.Parse(match.Groups["s"].Value, CultureInfo.InvariantCulture);

            // GPGGA carries no date, so the fixes are stamped onto the first representable
            // day; only the intervals between them are meaningful. The clock is only asked
            // once the position is known to be good, so a sentence that is skipped cannot
            // advance it and put a spurious day rollover into the fixes that follow.
            var timeOfDay =
                TimeSpan.FromHours(h) + TimeSpan.FromMinutes(m) + TimeSpan.FromSeconds(s);

            trackSegment.Waypoints.Add(
                new Waypoint(new Point(position), clock.Resolve(DateTime.MinValue, timeOfDay))
            );

            return true;
        }

        return false;
    }

    private bool ParseWaypoint(string line, GpsData data)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var match = Regex.Match(line, WPT_SENTENCE);
        if (match.Success)
        {
            if (!TryConvertPosition(match, null, out var position))
                return false;

            data.Waypoints.Add(new Waypoint(new Point(position), null, null, null));

            return true;
        }

        return false;
    }

    /// <summary>
    /// The position a matched sentence carries, with <paramref name="elevation" /> when the
    /// sentence quotes one, or <c>false</c> when its ordinates cannot be read.
    /// </summary>
    /// <remarks>
    /// A sentence whose fields matched but whose ordinates do not add up is skipped, exactly
    /// as a line that did not match at all already is. It used to throw instead - and since
    /// an NMEA log is a stream of sentences rather than one document, that lost every fix in
    /// the file, not just the bad one: a single truncated or corrupted line, which a log of
    /// any length is likely to contain, took the whole recording down with it.
    /// </remarks>
    private static bool TryConvertPosition(
        Match match,
        double? elevation,
        [NotNullWhen(true)] out Coordinate? position
    )
    {
        position = null;

        if (
            !TryConvertOrd(
                match.Groups["lat"].Value,
                match.Groups["latd"].Value,
                'S',
                2,
                out var lat
            )
            || !TryConvertOrd(
                match.Groups["lon"].Value,
                match.Groups["lond"].Value,
                'W',
                3,
                out var lon
            )
        )
            return false;

        try
        {
            position =
                elevation == null
                    ? new Coordinate(lat, lon)
                    : new CoordinateZ(lat, lon, elevation.Value);
        }
        catch (ArgumentOutOfRangeException)
        {
            // Degrees the sentence is free to quote but a position cannot hold. What is out
            // of range is the sentence, not an argument the caller passed, so this is not
            // raised at them as though it named a parameter of theirs. Longitude wrapping,
            // where it is switched on, still applies - the constructor decides that, which is
            // why the range is not pre-checked here.
            return false;
        }

        return true;
    }

    /// <summary>
    /// One ordinate, written as <paramref name="degreeDigits" /> digits of degrees followed
    /// by minutes, negated when its hemisphere letter is <paramref name="negative" />.
    /// </summary>
    private static bool TryConvertOrd(
        string ord,
        string dir,
        char negative,
        int degreeDigits,
        out double degrees
    )
    {
        degrees = 0;

        // Both fields are fixed-width in NMEA, so the split point is known - but the pattern
        // that matched the sentence does not enforce the width. A field too short to hold
        // both parts ran off the end of the string, or left the minutes empty for
        // double.Parse; either way it threw rather than being passed over.
        if (ord.Length <= degreeDigits)
            return false;

        var minutes = double.Parse(
            ord.Substring(degreeDigits),
            NumberStyles.Float,
            CultureInfo.InvariantCulture
        );

        degrees =
            double.Parse(
                ord.Substring(0, degreeDigits),
                NumberStyles.Float,
                CultureInfo.InvariantCulture
            )
            + minutes / 60;

        if (char.ToUpperInvariant(dir[0]) == negative)
            degrees = -degrees;

        return true;
    }
}
