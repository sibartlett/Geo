using System;
using Geo.Abstractions.Interfaces;

namespace Geo;

public class CoordinateZ : Coordinate, Is3D
{
    public CoordinateZ(double latitude, double longitude, double elevation)
        : base(latitude, longitude)
    {
        if (double.IsNaN(elevation) || double.IsInfinity(elevation))
            throw new ArgumentOutOfRangeException("elevation");

        Elevation = elevation;
    }

    public override bool Is3D => true;

    public double Elevation { get; }

    #region Equality methods

    public override bool Equals(object obj, SpatialEqualityOptions options)
    {
        var other = obj as Coordinate;

        if (ReferenceEquals(null, other))
            return false;

        var other2 = other as CoordinateZ;
        if (ReferenceEquals(null, other2))
            return false;

        if (options.UseElevation && !Elevation.Equals(other2.Elevation))
            return false;

        if (Latitude.Equals(other.Latitude))
        {
            if ((options.PoleCoordiantesAreEqual && Latitude.Equals(90d)) || Latitude.Equals(-90d))
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
            if (options.UseElevation)
                hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
