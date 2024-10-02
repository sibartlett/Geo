using System;
using Geo.Abstractions.Interfaces;

namespace Geo.Measure;

public struct Speed : IMeasure, IEquatable<Speed>, IComparable<Speed>
{
    public Speed(double metresPerSecond)
    {
        SiValue = metresPerSecond;
        Unit = SpeedUnit.Ms;
    }

    public Speed(double value, SpeedUnit unit)
    {
        SiValue = value.ConvertFrom(unit).To(SpeedUnit.Ms);
        Unit = unit;
    }

    public Speed(double metres, TimeSpan timeSpan)
    {
        Unit = SpeedUnit.Ms;
        if (
            Math.Abs(metres - 0d) < double.Epsilon
            || timeSpan == default
            || Math.Abs(timeSpan.TotalSeconds - 0) < double.Epsilon
        )
            SiValue = 0d;
        else
            SiValue = metres / timeSpan.TotalSeconds;
    }

    //public Speed(Distance distance, TimeSpan timeSpan) : this(distance.Value, timeSpan)
    //{
    //}

    public double Value => SiValue.ConvertTo(Unit);
    public double SiValue { get; }

    public SpeedUnit Unit { get; }

    public Speed ConvertTo(SpeedUnit unit)
    {
        return new Speed(SiValue.ConvertTo(unit), unit);
    }

    public override string ToString()
    {
        return UnitMetadata.For(Unit).Format(Value);
    }

    public string ToString(SpeedUnit unit)
    {
        return ConvertTo(unit).ToString();
    }

    public int CompareTo(Speed other)
    {
        if (Equals(other))
            return 0;
        return SiValue < other.SiValue ? -1 : 1;
    }

    //TODO
    //http://confluence.jetbrains.net/display/ReSharper/Compare+of+float+numbers+by+equality+operator

    public bool Equals(Speed other)
    {
        return SiValue.Equals(other.SiValue);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj))
            return false;
        return obj is Speed && Equals((Speed)obj);
    }

    public override int GetHashCode()
    {
        return SiValue.GetHashCode();
    }

    public static explicit operator Speed(int metersPerSecond)
    {
        return new Speed(metersPerSecond);
    }

    public static explicit operator Speed(long metersPerSecond)
    {
        return new Speed(metersPerSecond);
    }

    public static explicit operator Speed(double metersPerSecond)
    {
        return new Speed(metersPerSecond);
    }

    public static explicit operator Speed(float metersPerSecond)
    {
        return new Speed(metersPerSecond);
    }

    public static explicit operator Speed(decimal metersPerSecond)
    {
        return new Speed((double)metersPerSecond);
    }

    public static bool operator ==(Speed left, Speed right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(Speed left, Speed right)
    {
        return !left.Equals(right);
    }

    public static bool operator <(Speed left, Speed right)
    {
        return left.SiValue < right.SiValue;
    }

    public static bool operator >(Speed left, Speed right)
    {
        return left.SiValue > right.SiValue;
    }

    public static bool operator <=(Speed left, Speed right)
    {
        return left.SiValue <= right.SiValue;
    }

    public static bool operator >=(Speed left, Speed right)
    {
        return left.SiValue >= right.SiValue;
    }

    public static Speed operator +(Speed left, Speed right)
    {
        return new Speed(left.SiValue + right.SiValue);
    }

    public static Speed operator -(Speed left, Speed right)
    {
        return new Speed(left.SiValue - right.SiValue);
    }
}
