using System.Text.RegularExpressions;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.IO.Wkt;

namespace Geo.IO.Spatial4n
{
    public class Spatial4nReader
    {
        private readonly WktReader _wktReader = new WktReader();

        public ISpatial4nShape Read(string value)
        {
            if (string.IsNullOrEmpty(value))
                return null;

            ISpatial4nShape result;

            if (TryReadCircle(value, out result))
                return result;

            if (TryReadGeoPoint(value, out result))
                return result;

            if (TryReadPoint(value, out result))
                return result;

            if (TryReadEnvelope(value, out result))
                return result;

            return _wktReader.Read(value);
        }

        protected virtual double ConvertCircleRadius(double radius)
        {
            return radius.ToDegrees() * Constants.EarthMeanRadius;
        }

        private bool TryReadCircle(string value, out ISpatial4nShape result)
        {
            var match = Regex.Match(value,
                        @"Circle \s* \( \s* (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s+ d=(-?\d+.\d+?) \s* \)",
                        RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                result = new Circle(new Coordinate(double.Parse(match.Groups[2].Value), double.Parse(match.Groups[1].Value)), ConvertCircleRadius(double.Parse(match.Groups[3].Value)));
                return true;
            }
            result = null;
            return false;
        }

        private bool TryReadGeoPoint(string value, out ISpatial4nShape result)
        {
            var match = Regex.Match(value,
                        @"(-?\d+.\d+?) \s* , \s* (-?\d+.\d+?)",
                        RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                result = new Point(new Coordinate(double.Parse(match.Groups[1].Value), double.Parse(match.Groups[2].Value)));
                return true;
            }
            result = null;
            return false;
        }

        private bool TryReadPoint(string value, out ISpatial4nShape result)
        {
            var match = Regex.Match(value,
                        @"(-?\d+.\d+?) \s+ (-?\d+.\d+?)",
                        RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
            if (match.Success)
            {
                result = new Point(new Coordinate(double.Parse(match.Groups[2].Value), double.Parse(match.Groups[1].Value)));
                return true;
            }
            result = null;
            return false;
        }

        private bool TryReadEnvelope(string value, out ISpatial4nShape result)
        {
            var match = Regex.Match(value,
                        @"(-?\d+.\d+?) \s+ (-?\d+.\d+?) \s+ (-?\d+.\d+?) \s+ (-?\d+.\d+?)",
                        RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);

            if (match.Success)
            {
                result = new Envelope(
                    double.Parse(match.Groups[2].Value),
                    double.Parse(match.Groups[1].Value),
                    double.Parse(match.Groups[4].Value),
                    double.Parse(match.Groups[3].Value)
                    );
                return true;
            }
            result = null;
            return false;
        }
    }
}
