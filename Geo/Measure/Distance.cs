#nullable enable
using System;
using Geo.Abstractions.Interfaces;

namespace Geo.Measure;

public struct Distance : IMeasure, IEquatable<Distance>, IComparable<Distance>
{
    public Distance(double metres)
    {
        SiValue = metres;
        Unit = DistanceUnit.M;
    }

    public Distance(double value, DistanceUnit unit)
    {
        SiValue = value.ConvertFrom(unit).To(DistanceUnit.M);
        Unit = unit;
    }

    public double Value => SiValue.ConvertTo(Unit);
    public double SiValue { get; }

    public DistanceUnit Unit { get; }

    public Distance ConvertTo(DistanceUnit unit)
    {
        return new Distance(SiValue.ConvertTo(unit), unit);
    }

    public override string ToString()
    {
        return UnitMetadata.For(Unit).Format(Value);
    }

    public string ToString(DistanceUnit unit)
    {
        return ConvertTo(unit).ToString();
    }

    // Delegating to double keeps the ordering total. Deciding "not equal, and not less
    // than, therefore greater than" broke down for a NaN measure, which is neither: two
    // distances could each report themselves the greater, and a sort handed that pair
    // shuffled the values around it out of order (or gave up with "IComparer.Compare()
    // method returns inconsistent results"). Double.CompareTo agrees with Equals on the
    // cases that matter here - NaN compares equal to NaN, and zero to negative zero.
    public int CompareTo(Distance other)
    {
        return SiValue.CompareTo(other.SiValue);
    }

    public bool Equals(Distance other)
    {
        return SiValue.Equals(other.SiValue);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        return obj is Distance && Equals((Distance)obj);
    }

    public override int GetHashCode()
    {
        return SiValue.GetHashCode();
    }

    public static explicit operator Distance(int metersPerSecond)
    {
        return new Distance(metersPerSecond);
    }

    public static explicit operator Distance(long metersPerSecond)
    {
        return new Distance(metersPerSecond);
    }

    public static explicit operator Distance(double metersPerSecond)
    {
        return new Distance(metersPerSecond);
    }

    public static explicit operator Distance(float metersPerSecond)
    {
        return new Distance(metersPerSecond);
    }

    public static explicit operator Distance(decimal metersPerSecond)
    {
        return new Distance((double)metersPerSecond);
    }

    public static bool operator ==(Distance left, Distance right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Distance left, Distance right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Distance left, Distance right)
    {
        return left.SiValue < right.SiValue;
    }

    public static bool operator >(Distance left, Distance right)
    {
        return left.SiValue > right.SiValue;
    }

    public static bool operator <=(Distance left, Distance right)
    {
        return left.SiValue <= right.SiValue;
    }

    public static bool operator >=(Distance left, Distance right)
    {
        return left.SiValue >= right.SiValue;
    }

    public static Distance operator +(Distance left, Distance right)
    {
        return new Distance(left.SiValue + right.SiValue);
    }

    public static Distance operator -(Distance left, Distance right)
    {
        return new Distance(left.SiValue - right.SiValue);
    }

    public static Area operator *(Distance left, Distance right)
    {
        return new Area(left.SiValue * right.SiValue);
    }
}
