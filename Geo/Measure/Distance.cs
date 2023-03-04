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

    public int CompareTo(Distance other)
    {
        if (Equals(other))
            return 0;
        return SiValue < other.SiValue ? -1 : 1;
    }

    //TODO
    //http://confluence.jetbrains.net/display/ReSharper/Compare+of+float+numbers+by+equality+operator

    public bool Equals(Distance other)
    {
        return SiValue.Equals(other.SiValue);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
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