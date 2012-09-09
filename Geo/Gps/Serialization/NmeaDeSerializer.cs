using System;
using System.IO;
using System.Text.RegularExpressions;
using Geo.Geometries;

namespace Geo.Gps.Serialization
{
    public class NmeaDeSerializer : IGpsFileDeSerializer
    {
        private const string WPT_SENTENCE = @"^\$GPWPL,(?<lat>(?:\d+\.?\d*|\d*\.?\d+)),(?<latd>[NnSs]),(?<lon>(?:\d+\.?\d*|\d*\.?\d+)),(?<lond>[EeWw]),(?<id>[\d\w]+)\*[\d\w][\d\w]";
        private const string FIX_SENTENCE = @"^\$GPGGA\,(?<h>\d\d)(?<m>\d\d)(?<s>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<lat>(?:\d+\.?\d*|\d*\.?\d+))\,(?<latd>[NnSs])\,(?<lon>(?:\d+\.?\d*|\d*\.?\d+))\,(?<lond>[EeWw])\,(?<qual>[012])\,(?<sat>\d*)\,(?<hdop>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<alt>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<altU>[Mm])\,(?<geoid>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<geoidU>[Mm])\,(?<last>[+-]?(?:\d+\.?\d*|\d*\.?\d+))\,(?<stat>\d\d\d\d)\*[\d\w][\d\w]$";

        public GpsFileFormat[] FileFormats
        {
            get
            {
                return new[]
                    {
                        new GpsFileFormat("nmea", "NMEA"),
                    };
            }
        }

        public GpsFeatures SupportedFeatures { get { return GpsFeatures.TracksAndWaypoints; } }

        public bool CanDeSerialize(StreamWrapper streamWrapper)
        {
            streamWrapper.Position = 0;
            using (var reader = new StreamReader(streamWrapper))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                    if (Regex.IsMatch(line, FIX_SENTENCE) || Regex.IsMatch(line, WPT_SENTENCE))
                        return true;
            }
            return false;
        }

        public GpsData DeSerialize(StreamWrapper streamWrapper)
        {
            var data = new GpsData();
            var track = new LineString<Fix>();
            streamWrapper.Position = 0;
            using (var reader = new StreamReader(streamWrapper))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    if (ParseFix(line, track))
                        continue;
                    if (ParseWaypoint(line, data))
                        continue;
                }
            }

            if (!track.IsEmpty)
            {
                data.Tracks.Add(new Track());
                data.Tracks[0].Segments.Add(track);
            }
            return data;
        }

        private bool ParseFix(string line, LineString<Fix> data)
        {
            if (line.IsNullOrWhitespace())
                return false;

            var match = Regex.Match(line, FIX_SENTENCE);
            if (match.Success)
            {
                int h = int.Parse(match.Groups["h"].Value);
                int m = int.Parse(match.Groups["m"].Value);
                double s = double.Parse(match.Groups["s"].Value);
                double alt = double.Parse(match.Groups["alt"].Value);
                double lat = ConvertOrd(match.Groups["lat"].Value, match.Groups["latd"].Value);
                double lon = ConvertOrd(match.Groups["lon"].Value, match.Groups["lond"].Value);

                var fix = new Fix(lat, lon, alt, DateTime.MinValue.AddHours(h).AddMinutes(m).AddSeconds(s));
                data.Add(fix);

                return true;
            }
            return false;
        }

        private bool ParseWaypoint(string line, GpsData data)
        {
            if (line.IsNullOrWhitespace())
                return false;

            var match = Regex.Match(line, WPT_SENTENCE);
            if (match.Success)
            {
                double lat = ConvertOrd(match.Groups["lat"].Value, match.Groups["latd"].Value);
                double lon = ConvertOrd(match.Groups["lon"].Value, match.Groups["lond"].Value);
                var fix = new Point(lat, lon);
                data.Waypoints.Add(fix);

                return true;
            }
            return false;
        }

        private double ConvertOrd(string ord, string dir)
        {
            var d = Regex.IsMatch(dir, "^[NnEe]$") ? 1 : -1;
            var sub = Regex.IsMatch(dir, "^[NnSs]") ? 2 : 3;
            return (double.Parse(ord.Substring(0, sub)) + double.Parse(ord.Substring(sub, ord.Length - sub)) / 60) * d;
        }
    }
}

