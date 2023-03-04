using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using Geo.Gps.Metadata;

namespace Geo.Gps.Serialization;

public class IgcDeSerializer : IGpsFileDeSerializer
{
    private const string COORDINATE_REGEX =
        @"(?<d1>\d\d)(?<m1>\d\d\d\d\d)(?<dir1>[NnSs])(?<d2>\d\d\d)(?<m2>\d\d\d\d\d)(?<dir2>[EeWw])";

    private const string H_DATE_LINE_REGEX = @"^HFDTE(?<d>\d\d)(?<m>\d\d)(?<y>\d\d)$";
    private const string H_GLIDER_TYPE_REGEX = @"^HFGTYGLIDERTYPE:(?<value>.+)$";
    private const string H_GLIDER_REG_REGEX = @"^HFGIDGLIDERID:(?<value>.+)$";
    private const string H_CREW1_REGEX = @"^(?:HFPLTPILOT|HFPLTPILOTINCHARGE):(?<value>.+)$";
    private const string H_CREW2_REGEX = @"^HFCM2CREW2:(?<value>.+)$";

    private const string B_LINE_REGEX =
        @"^B(?<h>\d\d)(?<m>\d\d)(?<s>\d\d)(?<coord>\d\d\d\d\d\d\d[NnSs]\d\d\d\d\d\d\d\d[EeWw])(?<validAlt>[AaVv])(?<presAlt>\d\d\d\d\d)(?<gpsAlt>\d\d\d\d\d)";

    public GpsFileFormat[] FileFormats
    {
        get
        {
            return new[]
            {
                new GpsFileFormat("igc", "IGC", "http://carrier.csi.cam.ac.uk/forsterlewis/soaring/igc_file_format/")
            };
        }
    }

    public GpsFeatures SupportedFeatures => GpsFeatures.Tracks;

    public bool CanDeSerialize(StreamWrapper streamWrapper)
    {
        streamWrapper.Position = 0;
        using (var reader = new StreamReader(streamWrapper))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
                if (Regex.IsMatch(line, B_LINE_REGEX))
                    return true;
        }

        return false;
    }

    public GpsData DeSerialize(StreamWrapper streamWrapper)
    {
        var data = new GpsData();
        var date = default(DateTime);
        var trackSegment = new TrackSegment();

        streamWrapper.Position = 0;
        using (var reader = new StreamReader(streamWrapper))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (date == default)
                {
                    var match = Regex.Match(line, H_DATE_LINE_REGEX);
                    if (match.Success)
                    {
                        var d = int.Parse(match.Groups["d"].Value, CultureInfo.InvariantCulture);
                        var m = int.Parse(match.Groups["m"].Value, CultureInfo.InvariantCulture);
                        var y = int.Parse(match.Groups["y"].Value, CultureInfo.InvariantCulture);
                        var yn = int.Parse(DateTime.UtcNow.ToString("yy"), CultureInfo.InvariantCulture);
                        if (y > yn)
                            yn += 100;
                        var yd = yn - y;
                        date = new DateTime(DateTime.UtcNow.Year - yd, m, d);
                        continue;
                    }
                }

                if (ParseMetadata(data, x => x.Vehicle.Model, H_GLIDER_TYPE_REGEX, line))
                    continue;

                if (ParseMetadata(data, x => x.Vehicle.Identifier, H_GLIDER_REG_REGEX, line))
                    continue;

                if (ParseMetadata(data, x => x.Vehicle.Crew1, H_CREW1_REGEX, line))
                    continue;

                if (ParseMetadata(data, x => x.Vehicle.Crew2, H_CREW2_REGEX, line))
                    continue;

                if (ParseFix(line, trackSegment, date))
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

    private bool ParseMetadata(GpsData data, Func<GpsMetadata.MetadataKeys, string> attribute, string regex,
        string line)
    {
        var match = Regex.Match(line, regex);
        if (match.Success)
        {
            data.Metadata.Attribute(attribute, match.Groups["value"].Value);
            return true;
        }

        return false;
    }

    private bool ParseFix(string line, TrackSegment trackSegment, DateTime date)
    {
        if (string.IsNullOrWhiteSpace(line))
            return false;

        var match = Regex.Match(line, B_LINE_REGEX);
        if (match.Success)
        {
            var h = match.Groups["h"].Value;
            var m = match.Groups["m"].Value;
            var s = match.Groups["s"].Value;
            var coord = match.Groups["coord"].Value;
            var validAlt = match.Groups["validAlt"].Value;
            var presAlt = match.Groups["presAlt"].Value;
            var gpsAlt = match.Groups["gpsAlt"].Value;

            var cood = ParseCoordinate(coord);
            var waypoint = new Waypoint(cood.Latitude, cood.Longitude,
                double.Parse(gpsAlt, CultureInfo.InvariantCulture),
                date.AddHours(int.Parse(h, CultureInfo.InvariantCulture))
                    .AddMinutes(int.Parse(m, CultureInfo.InvariantCulture))
                    .AddSeconds(int.Parse(s, CultureInfo.InvariantCulture)));

            trackSegment.Waypoints.Add(waypoint);
            return true;
        }

        return false;
    }

    private Coordinate ParseCoordinate(string coord)
    {
        var match = Regex.Match(coord, COORDINATE_REGEX);

        if (match.Success)
        {
            var deg1 = double.Parse(match.Groups["d1"].Value, CultureInfo.InvariantCulture) +
                       double.Parse(match.Groups["m1"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
            var dir1 = Regex.IsMatch(match.Groups["dir1"].Value, "[Ss]") ? -1d : 1d;
            var deg2 = double.Parse(match.Groups["d2"].Value, CultureInfo.InvariantCulture) +
                       double.Parse(match.Groups["m2"].Value, CultureInfo.InvariantCulture) / 1000 / 60;
            var dir2 = Regex.IsMatch(match.Groups["dir2"].Value, "[Ww]") ? -1d : 1d;

            return new Coordinate(deg1 * dir1, deg2 * dir2);
        }

        throw new FormatException("Coordinate (" + coord + ") is not a supported format.");
    }
}