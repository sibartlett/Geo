using System;
using System.Text.RegularExpressions;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;

namespace Geo
{
    public class Coordinate : SpatialObject, IPosition
    {
        public Coordinate() : this(0, 0)
        {
        }

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

        public double Latitude { get; private set; }
        public double Longitude { get; private set; }

        public virtual bool Is3D
        {
            get { return false; }
        }

        public virtual bool IsMeasured
        {
            get { return false; }
        }

        public override string ToString()
        {
            return Latitude + ", " + Longitude;
        }

        Coordinate IPosition.GetCoordinate()
        {
            return this;
        }

        public Envelope GetBounds()
        {
            return new Envelope(Latitude, Longitude, Latitude, Longitude);
        }

        private const string CoordinateRegex
            = @"^[\(\[\{\s]*"
            + @"(?<Deg1>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)[°Dd\s]*(?<Min1>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[°'′Mm\s]*(?<Sec1>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[\""″\s]*(?<Dir1>[NnSsEeWw])?"
            + @"[,\s]+"
            + @"(?<Deg2>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)[°Dd\s]*(?<Min2>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[°'′Mm\s]*(?<Sec2>[+-]?(?:\d+\.?\d*|\d*\.?\d+)[\r\n]*)?[\""″\s]*(?<Dir2>[NnSsEeWw])?"
            + @"[\)\]\}\s]*$";

        public static Coordinate Parse(string coordinate)
        {
            if (coordinate == null)
                throw new ArgumentNullException("coordinate");

            if (string.IsNullOrWhiteSpace(coordinate))
                throw new ArgumentException("Value was empty", "coordinate");

            Coordinate result;
            if (!TryParse(coordinate, out result))
                throw new FormatException("Coordinate (" + coordinate + ") is not a supported format.");

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
            var match = Regex.Match(coordinate, CoordinateRegex);

            if (match.Success)
            {
                var deg1 = double.Parse(match.Groups["Deg1"].Value);
                var deg2 = double.Parse(match.Groups["Deg2"].Value);

                double temp;
                if (double.TryParse(match.Groups["Min1"].Value, out temp))
                    deg1 = deg1 + temp / 60;

                if (double.TryParse(match.Groups["Min2"].Value, out temp))
                    deg2 = deg2 + temp / 60;

                if (double.TryParse(match.Groups["Sec1"].Value, out temp))
                    deg1 = deg1 + temp / 3600;

                if (double.TryParse(match.Groups["Sec2"].Value, out temp))
                    deg2 = deg2 + temp / 3600;


                var dir1 = Regex.IsMatch(match.Groups["Dir1"].Value, "[Ss]") ? -1d : 1d;
                var dir2 = Regex.IsMatch(match.Groups["Dir2"].Value, "[Ww]") ? -1d : 1d;

                if (deg1 <= 90 && deg1 >= -90 || deg2 <= 180 && deg2 >= -180)
                {
                    result = new Coordinate(deg1 * dir1, deg2 * dir2);
                    return true;
                }
            }

            result = default(Coordinate);
            return false;
        }

        #region Equality methods

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as Coordinate;

            if (ReferenceEquals(null, other))
                return false;

            if (other.Is3D || other.IsMeasured)
                return false;

            if (Latitude.Equals(other.Latitude))
            {
                if (options.PoleCoordiantesAreEqual && Latitude.Equals(90d) || Latitude.Equals(-90d))
                    return true;

                if (Longitude.Equals(other.Longitude))
                    return true;

                if (options.AntiMeridianCoordinatesAreEqual)
                {
                    if (Longitude.Equals(180) && other.Longitude.Equals(-180) ||
                        Longitude.Equals(-180) && other.Longitude.Equals(180))
                        return true;
                }
            }

            return false;
        }

        public override bool Equals(object obj)
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

        public static bool operator ==(Coordinate left, Coordinate right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Coordinate left, Coordinate right)
        {
            return !(left == right);
        }

        #endregion
    }
}
