#nullable enable
using System;
using Geo.Abstractions.Interfaces;

namespace Geo;

public class CoordinateM : Coordinate, IsMeasured
{
    public CoordinateM(double latitude, double longitude, double measure)
        : base(latitude, longitude)
    {
        if (double.IsNaN(measure) || double.IsInfinity(measure))
            throw new ArgumentOutOfRangeException("measure");

        Measure = measure;
    }

    public override bool IsMeasured => true;

    public double Measure { get; }

    internal override double? MeasureOrNull => Measure;

    #region Equality methods

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
            if (options.UseM)
                hashCode = (hashCode * 397) ^ Measure.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
