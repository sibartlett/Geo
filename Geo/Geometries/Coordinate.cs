using System;
using System.Text.RegularExpressions;

namespace Geo.Geometries
{
    public class Coordinate : LatLngBase<Coordinate>
    {
        protected Coordinate() : base(0, 0)
        {
        }

        public Coordinate(double latitude, double longitude) : base(latitude, longitude)
        {
        }

        public Coordinate(double latitude, double longitude, double elevation) : base(latitude, longitude, elevation)
        {
        }

        private const string OrdRegex = @"^(?<Deg>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)[°Dd\s]*(?<Min>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[°'′Mm\s]*(?<Sec>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[\""″\s]*(?<Dir>[NnSsEeWw])?$";
        
        private static string[] SplitCoordinateString(string coordinate)
        {
            if (coordinate.IsNullOrWhitespace())
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

            return new[] { ordinates[0].Trim(), ordinates[1].Trim() };
        }

        private static bool TryParseOrdinateInternal(string ordinateString, int type, out double ordinate)
        {
            ordinate = default(double);
            if (ordinateString.IsNullOrWhitespace())
                return false;

            ordinateString = ordinateString.Trim();

            var match = Regex.Match(ordinateString, OrdRegex);

            if (match.Success)
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
                            type = 0;
                            break;
                        case "S":
                        case "s":
                            type = 0;
                            direction = -1;
                            break;
                        case "E":
                        case "e":
                            type = 1;
                            break;
                        case "W":
                        case "w":
                            type = 1;
                            direction = -1;
                            break;
                    }
                }

                if (string.IsNullOrEmpty(rMin) && string.IsNullOrEmpty(rSec))
                {
                    int test;
                    var maxLength = 2 + type;
                    if (int.TryParse(rDeg, out test))
                        if (rDeg.Length > maxLength)
                        {
                            if (rDeg.Length == 5 + maxLength)
                            {
                                rMin = rDeg.Substring(maxLength, 2) + "." + rDeg.Substring(maxLength + 2, 3);
                                rDeg = rDeg.Substring(0, maxLength);
                            }
                        }
                }

                double deg;

                if (double.TryParse(rDeg, out deg))
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

        private static bool Validate(double ordinate, int type, out double result)
        {
            if (type == 0 && ordinate <= 90 && ordinate >= -90 ||
                type == 1 && ordinate <= 180 && ordinate >= -180)
            {
                result = ordinate;
                return true;
            }
            result = default(double);
            return false;
        }

        public static Coordinate Parse(string coordinate)
        {
            if (coordinate == null)
                throw new ArgumentNullException("coordinate");

            if (coordinate.IsNullOrWhitespace())
                throw new ArgumentException("Value was empty", "coordinate");

            Coordinate result;
            if (!TryParse(coordinate, out result))
                throw new FormatException("Coordinate (" + coordinate + ") is a not supported format.");

            return result;
        }

        public static Coordinate TryParse(string coordinate)
        {
            Coordinate result;
            TryParse(coordinate, out result);
            return result;
        }

        public static bool TryParse(string coordinate, out Coordinate result)
        {
            var a = SplitCoordinateString(coordinate);
            if (a != null)
            {
                double lat;
                double lon;
                if (TryParseOrdinateInternal(a[0], 0, out lat))
                    if (TryParseOrdinateInternal(a[1], 1, out lon))
                    {
                        result = new Coordinate(lat, lon);
                        return true;
                    }
            }
            result = default(Coordinate);
            return false;
        }

        public Point ToPoint()
        {
            return new Point(this);
        }

        public static implicit operator Coordinate(Point point)
        {
            return point.GetCoordinate();
        }

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return ReferenceEquals(left, null) || ReferenceEquals(right, null) || !left.Equals(right);
        }
    }
}
