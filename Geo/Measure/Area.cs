using System;
using Geo.Abstractions.Interfaces;

namespace Geo.Measure;

public struct Area : IMeasure, IEquatable<Area>, IComparable<Area>
{
    public Area(double metres)
    {
        SiValue = metres;
        Unit = DistanceUnit.M;
    }

    public Area(double value, DistanceUnit unit)
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

    public int CompareTo(Area other)
    {
        if (Equals(other))
            return 0;
        return SiValue < other.SiValue ? -1 : 1;
    }

    //TODO
    //http://confluence.jetbrains.net/display/ReSharper/Compare+of+float+numbers+by+equality+operator

    public bool Equals(Area other)
    {
        return SiValue.Equals(other.SiValue);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        return obj is Distance && Equals((Distance)obj);
    }

    public override int GetHashCode()
    {
        return SiValue.GetHashCode();
    }

    public static explicit operator Area(int metersPerSecond)
    {
        return new Area(metersPerSecond);
    }

    public static explicit operator Area(long metersPerSecond)
    {
        return new Area(metersPerSecond);
    }

    public static explicit operator Area(double metersPerSecond)
    {
        return new Area(metersPerSecond);
    }

    public static explicit operator Area(float metersPerSecond)
    {
        return new Area(metersPerSecond);
    }

    public static explicit operator Area(decimal metersPerSecond)
    {
        return new Area((double)metersPerSecond);
    }

    public static bool operator ==(Area left, Area right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Area left, Area right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Area left, Area right)
    {
        return left.SiValue < right.SiValue;
    }

    public static bool operator >(Area left, Area right)
    {
        return left.SiValue > right.SiValue;
    }

    public static bool operator <=(Area left, Area right)
    {
        return left.SiValue <= right.SiValue;
    }

    public static bool operator >=(Area left, Area right)
    {
        return left.SiValue >= right.SiValue;
    }

    public static Area operator +(Area left, Area right)
    {
        return new Area(left.SiValue + right.SiValue);
    }

    public static Area operator -(Area left, Area right)
    {
        return new Area(left.SiValue - right.SiValue);
    }
}
