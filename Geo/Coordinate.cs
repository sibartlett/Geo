#nullable enable
using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;

namespace Geo;

public class Coordinate : SpatialObject, IPosition
{
    // One ordinate's number: an optional sign, then digits with optional decimals, or a
    // bare fraction. Spelling it this way rather than as (?:\d+\.?\d*|\d*\.?\d+) matches
    // exactly the same strings but only one way each. The two branches of the old form
    // both accepted a plain run of digits, and could split one between \d+ and \d* at any
    // point, so six of these in sequence gave the engine an exponential number of ways to
    // carve up a long run before admitting the string was not a coordinate at all: forty
    // digits took over five seconds, and each further digit multiplied that.
    private const string OrdinateNumber = @"[+-]?(?:\d+(?:\.\d*)?|\.\d+)";

    // The hemisphere letter may lead the ordinate ("N51 30.0") as well as follow it
    // ("51 30.0N"): aviation and marine sources overwhelmingly write it in front, and a
    // string of theirs did not parse at all before. Both spellings are optional, and an
    // ordinate that carries the letter on both sides has to say the same thing twice.
    private static readonly string CoordinateRegex =
        @"^[\(\[\{\s]*"
        + $@"(?<Pre1>[NnSsEeWw])?\s*(?<Deg1>{OrdinateNumber}[\r\n]*)[°Dd\s]*(?<Min1>{OrdinateNumber}[\r\n]*)?[°'′Mm\s]*(?<Sec1>{OrdinateNumber}[\r\n]*)?[\""″\s]*(?<Dir1>[NnSsEeWw])?"
        + @"[,\s]+"
        + $@"(?<Pre2>[NnSsEeWw])?\s*(?<Deg2>{OrdinateNumber}[\r\n]*)[°Dd\s]*(?<Min2>{OrdinateNumber}[\r\n]*)?[°'′Mm\s]*(?<Sec2>{OrdinateNumber}[\r\n]*)?[\""″\s]*(?<Dir2>[NnSsEeWw])?"
        + @"[\)\]\}\s]*$";

    // A backstop for what the rewrite above cannot reach. Degrees, minutes and seconds are
    // separated by characters that are all optional, so a long run of digits can still be
    // divided between the three fields polynomially many ways; that is no longer
    // exponential, but several hundred digits would still take seconds. Nothing any
    // coordinate notation produces comes within orders of magnitude of this budget, and a
    // string that exhausts it is reported as one that does not parse rather than being
    // allowed to occupy the caller indefinitely.
    private static readonly TimeSpan MatchTimeout = TimeSpan.FromMilliseconds(250);

    public Coordinate()
        : this(0, 0) { }

    public Coordinate(double latitude, double longitude)
    {
        if (latitude > 90 || latitude < -90)
            throw new ArgumentOutOfRangeException("latitude");

        if (GeoContext.Current.LongitudeWrapping)
        {
            while (longitude > 180)
                longitude -= 360;
            while (longitude < -180)
                longitude += 360;
        }

        if (longitude > 180 || longitude < -180)
            throw new ArgumentOutOfRangeException("longitude");

        Latitude = latitude;
        Longitude = longitude;
    }

    public double Latitude { get; }
    public double Longitude { get; }

    public virtual bool Is3D => false;

    public virtual bool IsMeasured => false;

    /// <summary>
    /// The elevation this coordinate carries, or <c>null</c> when it carries none.
    /// </summary>
    internal virtual double? ElevationOrNull => null;

    /// <summary>
    /// The measure this coordinate carries, or <c>null</c> when it carries none.
    /// </summary>
    internal virtual double? MeasureOrNull => null;

    Coordinate IPosition.GetCoordinate()
    {
        return this;
    }

    public override string ToString()
    {
        // Invariant culture, so the output stays parseable by Parse/TryParse: a culture
        // that writes the decimal separator as a comma would otherwise collide with the
        // comma separating the two ordinates ("51,5, -0,12").
        return Latitude.ToString(CultureInfo.InvariantCulture)
            + ", "
            + Longitude.ToString(CultureInfo.InvariantCulture);
    }

    public Envelope GetBounds()
    {
        return new Envelope(Latitude, Longitude, Latitude, Longitude);
    }

    public static Coordinate Parse(string coordinate)
    {
        if (coordinate == null)
            throw new ArgumentNullException("coordinate");

        if (string.IsNullOrWhiteSpace(coordinate))
            throw new ArgumentException("Value was empty", "coordinate");

        if (!TryParse(coordinate, out var result))
            throw new FormatException("Coordinate (" + coordinate + ") is not a supported format.");

        return result;
    }

    public static Coordinate? TryParse(string coordinate)
    {
        TryParse(coordinate, out var result);
        return result;
    }

    public static bool TryParse(string coordinate, [NotNullWhen(true)] out Coordinate? result)
    {
        // TryParse reports failure rather than throwing, so a null input is just
        // another value that does not parse. Parse still rejects it up front with
        // an ArgumentNullException.
        if (coordinate == null)
        {
            result = default;
            return false;
        }

        Match match;
        try
        {
            match = Regex.Match(coordinate, CoordinateRegex, RegexOptions.None, MatchTimeout);
        }
        catch (RegexMatchTimeoutException)
        {
            result = default;
            return false;
        }

        if (
            match.Success
            && TryParseHemisphere(match, "Pre1", "Dir1", 'N', 'S', out var dir1)
            && TryParseHemisphere(match, "Pre2", "Dir2", 'E', 'W', out var dir2)
        )
        {
            var deg1 = ParseOrdinate(match, "Deg1", "Min1", "Sec1");
            var deg2 = ParseOrdinate(match, "Deg2", "Min2", "Sec2");

            if (deg1 is <= 90 and >= -90 && deg2 is <= 180 and >= -180)
            {
                result = new Coordinate(deg1 * dir1, deg2 * dir2);
                return true;
            }
        }

        result = default;
        return false;
    }

    /// <summary>
    /// The sign the ordinate's hemisphere letter asks for — <c>1</c> for
    /// <paramref name="positive" /> (and for no letter at all), <c>-1</c> for
    /// <paramref name="negative" />. Returns <c>false</c> when the letters cannot be
    /// honoured, which fails the parse.
    /// </summary>
    /// <remarks>
    /// A letter naming the other axis — an E or a W where a latitude belongs — is rejected
    /// rather than ignored. Ignoring it dropped the hemisphere silently, so "0.12W, 51.5N"
    /// parsed as if both ordinates were positive and put the position on the wrong side of
    /// the meridian; refusing it leaves the caller to swap the ordinates into the
    /// latitude-then-longitude order this method reads them in.
    /// </remarks>
    private static bool TryParseHemisphere(
        Match match,
        string prefixGroup,
        string suffixGroup,
        char positive,
        char negative,
        out double sign
    )
    {
        sign = 1;

        var prefix = match.Groups[prefixGroup].Value;
        var suffix = match.Groups[suffixGroup].Value;

        if (prefix.Length == 0 && suffix.Length == 0)
            return true;

        // Written on both sides, the two can only be a restatement of one hemisphere.
        if (
            prefix.Length > 0
            && suffix.Length > 0
            && char.ToUpperInvariant(prefix[0]) != char.ToUpperInvariant(suffix[0])
        )
            return false;

        var letter = char.ToUpperInvariant(prefix.Length > 0 ? prefix[0] : suffix[0]);

        if (letter == positive)
            return true;

        if (letter == negative)
        {
            sign = -1;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Assembles one ordinate from its degrees, minutes and seconds groups.
    /// </summary>
    /// <remarks>
    /// The minutes and seconds are added to the <em>magnitude</em> of the degrees and the
    /// sign is applied once at the end, because a degrees field of "-0" parses to negative
    /// zero, which is not less than zero. Deciding the direction by testing the parsed
    /// value therefore read "-0 7 12" as travelling north/east of zero, and every
    /// coordinate in the (-1, 0) degree band - which is where most of western Europe's
    /// longitudes sit - came back on the wrong side of the meridian, up to 111 km out.
    /// The sign is taken from the text instead, which is the only place it survives.
    /// </remarks>
    private static double ParseOrdinate(
        Match match,
        string degreesGroup,
        string minutesGroup,
        string secondsGroup
    )
    {
        var text = match.Groups[degreesGroup].Value;
        var degrees = double.Parse(text, CultureInfo.InvariantCulture);
        var negative = degrees < 0 || text.TrimStart().StartsWith("-", StringComparison.Ordinal);

        var magnitude = Math.Abs(degrees);

        if (TryParseOrdinatePart(match, minutesGroup, out var minutes))
            magnitude += minutes / 60;

        if (TryParseOrdinatePart(match, secondsGroup, out var seconds))
            magnitude += seconds / 3600;

        return negative ? -magnitude : magnitude;
    }

    // The minutes and seconds groups must be read with the same invariant culture as the
    // degrees group above. Left to the current culture they would parse a decimal point
    // as a thousands separator wherever the comma and point are swapped, turning
    // "51 30.5' N" into 51 degrees 305 minutes — five degrees, or ~550 km, adrift.
    private static bool TryParseOrdinatePart(Match match, string group, out double value)
    {
        return double.TryParse(
            match.Groups[group].Value,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.InvariantCulture,
            out value
        );
    }

    #region Equality methods

    // Every coordinate type compares the same way, so the comparison lives here rather
    // than being repeated (and drifting) in CoordinateZ/CoordinateM/CoordinateZM.
    public override bool Equals(object? obj, SpatialEqualityOptions options)
    {
        var other = obj as Coordinate;

        return !ReferenceEquals(null, other)
            && OrdinatesEqual(other, options)
            && PositionEquals(other, options);
    }

    /// <summary>
    /// Compares the elevation and the measure, each only when <paramref name="options" />
    /// asks for it.
    /// </summary>
    /// <remarks>
    /// An ordinate that is being compared has to be present on both sides or absent from
    /// both, so under the default options a <see cref="CoordinateZ" /> is still not the
    /// same as a plain <see cref="Coordinate" />. An ordinate that is <em>not</em> being
    /// compared is ignored entirely, which is what lets a 2D comparison match a
    /// <see cref="CoordinateZ" /> against a <see cref="Coordinate" /> at the same
    /// position - exactly what <see cref="GetHashCode(SpatialEqualityOptions)" /> has
    /// always done, and what <see cref="Linq.EnumerableExtensions.Distinct2D" /> and
    /// <see cref="Linq.Spatial2DComparer{TSource}" /> need in order to work at all.
    /// </remarks>
    private bool OrdinatesEqual(Coordinate other, SpatialEqualityOptions options)
    {
        if (options.UseElevation && !Nullable.Equals(ElevationOrNull, other.ElevationOrNull))
            return false;

        if (options.UseM && !Nullable.Equals(MeasureOrNull, other.MeasureOrNull))
            return false;

        return true;
    }

    private bool PositionEquals(Coordinate other, SpatialEqualityOptions options)
    {
        if (Latitude.Equals(other.Latitude))
        {
            if (options.PoleCoordiantesAreEqual && (Latitude.Equals(90d) || Latitude.Equals(-90d)))
                return true;

            if (Longitude.Equals(other.Longitude))
                return true;

            if (options.AntiMeridianCoordinatesAreEqual)
                if (
                    (Longitude.Equals(180) && other.Longitude.Equals(-180))
                    || (Longitude.Equals(-180) && other.Longitude.Equals(180))
                )
                    return true;
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj, GeoContext.Current.EqualityOptions);
    }

    public override int GetHashCode()
    {
        return base.GetHashCode();
    }

    public override int GetHashCode(SpatialEqualityOptions options)
    {
        unchecked
        {
            var latitude = Latitude;
            var longitude = Longitude;

            if (options.PoleCoordiantesAreEqual && (Latitude.Equals(90) || Latitude.Equals(-90)))
                longitude = 0;
            else if (options.AntiMeridianCoordinatesAreEqual && Longitude.Equals(-180))
                longitude = 180;

            var hashCode = latitude.GetHashCode();
            hashCode = (hashCode * 397) ^ longitude.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Coordinate? left, Coordinate? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(Coordinate? left, Coordinate? right)
    {
        return !(left == right);
    }

    #endregion
}
