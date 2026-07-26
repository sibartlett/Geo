#nullable enable
using System;
using Geo.Abstractions.Interfaces;

namespace Geo;

public class CoordinateZM : Coordinate, Is3D, IsMeasured
{
    public CoordinateZM(double latitude, double longitude, double elevation, double measure)
        : base(latitude, longitude)
    {
        if (double.IsNaN(elevation) || double.IsInfinity(elevation))
            throw new ArgumentOutOfRangeException("elevation");

        if (double.IsNaN(measure) || double.IsInfinity(measure))
            throw new ArgumentOutOfRangeException("measure");

        Elevation = elevation;
        Measure = measure;
    }

    public override bool Is3D => true;

    public override bool IsMeasured => true;

    public double Elevation { get; }
    public double Measure { get; }

    internal override double? ElevationOrNull => Elevation;

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
            var hashCode = GetPositionHashCode();
            if (options.UseElevation)
                hashCode = (hashCode * 397) ^ Elevation.GetHashCode();
            if (options.UseM)
                hashCode = (hashCode * 397) ^ Measure.GetHashCode();
            return hashCode;
        }
    }

    #endregion
}
