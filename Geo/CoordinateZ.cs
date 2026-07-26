#nullable enable
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

    internal override double? ElevationOrNull => Elevation;

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
            return hashCode;
        }
    }

    #endregion
}
