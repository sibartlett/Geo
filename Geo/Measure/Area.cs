using System;
using Geo.Abstractions.Interfaces;

namespace Geo.Measure;

public struct Area : IMeasure, IEquatable<Area>, IComparable<Area>
{
    public Area(double squareMetres)
    {
        SiValue = squareMetres;
        Unit = AreaUnit.M;
    }

    public Area(double value, AreaUnit unit)
    {
        SiValue = value.ConvertFrom(unit).To(AreaUnit.M);
        Unit = unit;
    }

    public double Value => SiValue.ConvertTo(Unit);
    public double SiValue { get; }

    public AreaUnit Unit { get; }

    public Area ConvertTo(AreaUnit unit)
    {
        return new Area(SiValue.ConvertTo(unit), unit);
    }

    public override string ToString()
    {
        return UnitMetadata.For(Unit).Format(Value);
    }

    public string ToString(AreaUnit unit)
    {
        return ConvertTo(unit).ToString();
    }

    // Delegating to double keeps the ordering total; see Distance.CompareTo for why
    // deriving "greater than" from "neither equal nor less than" does not hold.
    public int CompareTo(Area other)
    {
        return SiValue.CompareTo(other.SiValue);
    }

    public bool Equals(Area other)
    {
        return SiValue.Equals(other.SiValue);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        return obj is Area && Equals((Area)obj);
    }

    public override int GetHashCode()
    {
        return SiValue.GetHashCode();
    }

    public static explicit operator Area(int squareMetres)
    {
        return new Area(squareMetres);
    }

    public static explicit operator Area(long squareMetres)
    {
        return new Area(squareMetres);
    }

    public static explicit operator Area(double squareMetres)
    {
        return new Area(squareMetres);
    }

    public static explicit operator Area(float squareMetres)
    {
        return new Area(squareMetres);
    }

    public static explicit operator Area(decimal squareMetres)
    {
        return new Area((double)squareMetres);
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
