using System;
using System.Text.RegularExpressions;

namespace Geo
{
    internal static class GeoUtil
    {
        private const string OrdRegex = @"^(?<Deg>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)[°Dd\s]*(?<Min>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[°'′Mm\s]*(?<Sec>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[\""″\s]*(?<Dir>[NnSsEeWw])?$";
        
        internal static bool TryParseOrdinateInternal(string ordinateString, OrdinateType type, out double ordinate)
        {
            ordinate = default(double);
            if (string.IsNullOrWhiteSpace(ordinateString))
                return false;

            ordinateString = ordinateString.Trim();

            var match = Regex.Match(ordinateString, OrdRegex);

            if(match.Success)
            {
                var rDeg = match.Groups["Deg"].Value;
                var rMin = match.Groups["Min"].Value;
                var rSec = match.Groups["Sec"].Value;
                var rDir = match.Groups["Dir"].Value;

                int direction = 1;
                if (!string.IsNullOrEmpty(rDir))
                {
                    switch (rDir)
                    {
                        case "N":
                        case "n":
                            type = OrdinateType.Latitude;
                            break;
                        case "S":
                        case "s":
                            type = OrdinateType.Latitude;
                            direction = -1;
                            break;
                        case "E":
                        case "e":
                            type = OrdinateType.Longitude;
                            break;
                        case "W":
                        case "w":
                            type = OrdinateType.Longitude;
                            direction = -1;
                            break;
                    }
                }

                if(string.IsNullOrEmpty(rMin) && string.IsNullOrEmpty(rSec))
                {
                    int test;
                    var maxLength = type == OrdinateType.Latitude ? 2 : 3;
                    if (int.TryParse(rDeg, out test))
                        if(rDeg.Length > maxLength)
                        {
                            if(rDeg.Length == 5 + maxLength)
                            {
                                rMin = rDeg.Substring(maxLength, 2) + "." + rDeg.Substring(maxLength + 2, 3);
                                rDeg = rDeg.Substring(0, maxLength);
                            }
                        }  
                }

                double deg;

                if(double.TryParse(rDeg, out deg))
                {
                    double min;
                    double sec;
                    double.TryParse(rMin, out min);
                    double.TryParse(rSec, out sec);

                    var result = (deg + min / 60 + sec / 3600) * direction;

                    if (Validate(result, type, out ordinate))
                            return true;
                }
            }
            return false;
        }

        private static bool Validate(double ordinate, OrdinateType type, out double result)
        {
            if (type == OrdinateType.Latitude && ordinate <= 90 && ordinate >= -90 ||
                type != OrdinateType.Latitude && ordinate <= 180 && ordinate >= -180)
            {
                result = ordinate;
                return true;
            }
            result = default(double);
            return false;
        }


        internal enum OrdinateType
        {
            Unknown,
            Latitude,
            Longitude,
        }

        public static Tuple<string,string> SplitCoordinateString(string coordinate)
        {
            if(string.IsNullOrWhiteSpace(coordinate))
                return null;

            coordinate = coordinate.Trim();
            string[] ordinates = null;

            if (Regex.IsMatch(coordinate, "^[^,]*,[^,]*$"))
            {
                ordinates = coordinate.Split(',');
            }

            else if (Regex.IsMatch(coordinate, "^[^\\s]*[\\s]+[^\\s]*$"))
            {
                var index = Regex.Match(coordinate, "\\s").Index + 1;
                ordinates = new[]
                    {
                        coordinate.Substring(0, index),
                        coordinate.Substring(index, coordinate.Length - index)
                    };
            }

            else if (Regex.IsMatch(coordinate, "^[^NnSsEeWw]*[NnSs][^NnSsEeWw]*[EeWw]$"))
            {
                var index = Regex.Match(coordinate, "[NnSs]").Index + 1;
                ordinates = new[]
                    {
                        coordinate.Substring(0, index),
                        coordinate.Substring(index, coordinate.Length - index)
                    };
            }

            if (ordinates == null)
                return null;

            return new Tuple<string, string>(
                ordinates[0].Trim(), ordinates[1].Trim()
            );
        }
    }
}
