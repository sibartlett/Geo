using System;
using System.IO;
using System.Text.RegularExpressions;
using Geo.Geometries;
using Geo.Gps.Metadata;

namespace Geo.Gps.Serialization
{
    public class IgcDeSerializer : IGpsFileDeSerializer
    {
        private const string H_DATE_LINE_REGEX = @"^HFDTE(?<d>\d\d)(?<m>\d\d)(?<y>\d\d)$";
        private const string H_GLIDER_TYPE_REGEX = @"^HFGTYGLIDERTYPE:(?<value>.+)$";
        private const string H_GLIDER_REG_REGEX = @"^HFGIDGLIDERID:(?<value>.+)$";
        private const string H_CREW1_REGEX = @"^(?:HFPLTPILOT|HFPLTPILOTINCHARGE):(?<value>.+)$";
        private const string H_CREW2_REGEX = @"^HFCM2CREW2:(?<value>.+)$";
        private const string B_LINE_REGEX = @"^B(?<h>\d\d)(?<m>\d\d)(?<s>\d\d)(?<coord>\d\d\d\d\d\d\d[NnSs]\d\d\d\d\d\d\d\d[EeWw])(?<validAlt>[AaVv])(?<presAlt>\d\d\d\d\d)(?<gpsAlt>\d\d\d\d\d)";

        public GpsFeatures SupportedFeatures { get { return GpsFeatures.Tracks;} }

        public string[] FileExtensions { get { return new[] { "igc" }; } }

        public Uri FileFormatSpecificationUri { get { return new Uri("http://carrier.csi.cam.ac.uk/forsterlewis/soaring/igc_file_format/"); } }

        public bool CanDeSerialize(StreamWrapper streamWrapper)
        {
            streamWrapper.Position = 0;
            using (var reader = new StreamReader(streamWrapper))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (Regex.IsMatch(line, B_LINE_REGEX))
                        return true;
                }
            }
            return false;
        }

        private bool ParseMetadata(GpsData data, Func<GpsMetadata.MetadataKeys, string> attribute, string regex, string line)
        {
            var match = Regex.Match(line, regex);
            if (match.Success)
            {
                data.Metadata.Attribute(attribute, match.Groups["value"].Value);
                return true;
            }
            return false;
        }

        public GpsData DeSerialize(StreamWrapper streamWrapper)
        {
            var data = new GpsData();
            DateTime date = default(DateTime);
            var track = new LineString<Fix>();

            streamWrapper.Position = 0;
            using (var reader = new StreamReader(streamWrapper))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (date == default(DateTime))
                    {
                        var match = Regex.Match(line, H_DATE_LINE_REGEX);
                        if (match.Success)
                        {
                            var d = int.Parse(match.Groups["d"].Value);
                            var m = int.Parse(match.Groups["m"].Value);
                            var y = int.Parse(match.Groups["y"].Value);
                            var yn = int.Parse(DateTime.UtcNow.ToString("yy"));
                            if (y > yn)
                                yn += 100;
                            var yd = yn - y;
                            date = new DateTime(DateTime.UtcNow.Year -yd, m, d);
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

                    if (ParseFix(line, track, date))
                        continue;
                }
            }

            if(!track.IsEmpty)
            {
                data.Tracks.Add(new Track());
                data.Tracks[0].Segments.Add(track);
            }

            return data;
        }

        private bool ParseFix(string line, LineString<Fix> data, DateTime date)
        {
            if (line.IsNullOrWhitespace())
                return false;

            var match = Regex.Match(line, B_LINE_REGEX);
            if (match.Success)
            {
                string h = match.Groups["h"].Value;
                string m = match.Groups["m"].Value;
                string s = match.Groups["s"].Value;
                string coord = match.Groups["coord"].Value;
                string validAlt = match.Groups["validAlt"].Value;
                string presAlt = match.Groups["presAlt"].Value;
                string gpsAlt = match.Groups["gpsAlt"].Value;

                var cood = Point.ParseCoordinate(coord);
                var fix = new Fix(cood.Latitude, cood.Longitude, double.Parse(gpsAlt), date.AddHours(int.Parse(h)).AddMinutes(int.Parse(m)).AddSeconds(int.Parse(s)));

                data.Add(fix);
                return true;
            }
            return false;
        }
    }
}
