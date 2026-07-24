#nullable enable
using System;
using Geo.Abstractions.Interfaces;
using Geo.Measure;

namespace Geo;

public class Envelope : IHasArea, IHasLength, IEquatable<Envelope>
{
    public Envelope(double minLat, double minLon, double maxLat, double maxLon)
    {
        MinLat = minLat;
        MinLon = minLon;
        MaxLat = maxLat;
        MaxLon = maxLon;
    }

    public double MinLat { get; }
    public double MinLon { get; }
    public double MaxLat { get; }
    public double MaxLon { get; }

    public Area GetArea()
    {
        return GeoContext.Current.GeodeticCalculator.CalculateArea(this);
    }

    public Distance GetLength()
    {
        return GeoContext.Current.GeodeticCalculator.CalculateLength(this);
    }

    public Envelope Combine(Envelope? other)
    {
        if (other == null)
            return this;

        return new Envelope(
            Math.Min(MinLat, other.MinLat),
            Math.Min(MinLon, other.MinLon),
            Math.Max(MaxLat, other.MaxLat),
            Math.Max(MaxLon, other.MaxLon)
        );
    }

    public bool Intersects(Envelope envelope)
    {
        // Two axis-aligned envelopes overlap when their latitude spans overlap and
        // their longitude spans overlap. Testing the spans directly (rather than
        // whether a corner of one lies inside the other) correctly handles cross-shaped
        // overlaps, where neither envelope has a corner inside the other, as well as
        // identical envelopes and envelopes that touch along an edge.
        return MinLat <= envelope.MaxLat
            && MaxLat >= envelope.MinLat
            && MinLon <= envelope.MaxLon
            && MaxLon >= envelope.MinLon;
    }

    public bool Contains(Envelope? envelope)
    {
        // Inclusive of the boundary: an envelope contains one whose span lies within
        // its own, edges included, so an envelope contains itself (matching the OGC
        // convention and consistent with Intersects).
        return envelope != null
            && envelope.MinLat >= MinLat
            && envelope.MaxLat <= MaxLat
            && envelope.MinLon >= MinLon
            && envelope.MaxLon <= MaxLon;
    }

    public bool Contains(Coordinate coordinate)
    {
        // Inclusive of the boundary: a coordinate on an edge or corner is contained.
        return coordinate.Latitude >= MinLat
            && coordinate.Latitude <= MaxLat
            && coordinate.Longitude >= MinLon
            && coordinate.Longitude <= MaxLon;
    }

    public bool Contains(IGeometry? geometry)
    {
        return geometry != null && Contains(geometry.GetBounds());
    }

    #region Equality methods

    public bool Equals(Envelope? other)
    {
        return !ReferenceEquals(null, other)
            && MinLat.Equals(other.MinLat)
            && MinLon.Equals(other.MinLon)
            && MaxLat.Equals(other.MaxLat)
            && MaxLon.Equals(other.MaxLon);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        if (ReferenceEquals(this, obj))
            return true;
        if (obj.GetType() != GetType())
            return false;
        return Equals((Envelope)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            var hashCode = MinLat.GetHashCode();
            hashCode = (hashCode * 397) ^ MinLon.GetHashCode();
            hashCode = (hashCode * 397) ^ MaxLat.GetHashCode();
            hashCode = (hashCode * 397) ^ MaxLon.GetHashCode();
            return hashCode;
        }
    }

    public static bool operator ==(Envelope? left, Envelope? right)
    {
        if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
            return true;
        return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
    }

    public static bool operator !=(Envelope? left, Envelope? right)
    {
        return !(left == right);
    }

    #endregion
}
