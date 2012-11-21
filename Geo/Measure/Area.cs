using System;
using Geo.Abstractions.Interfaces;

namespace Geo.Measure
{
    public struct Area : IMeasure, IEquatable<Area>, IComparable<Area>
    {
        private readonly double _siValue;
        private readonly DistanceUnit _unit;

        public Area(double metres)
        {
            _siValue = metres;
            _unit = DistanceUnit.M;
        }

        public Area(double value, DistanceUnit unit)
        {
            _siValue = value.ConvertFrom(unit).To(DistanceUnit.M);
            _unit = unit;
        }

        public double Value { get { return _siValue.ConvertTo(_unit); } }
        public double SiValue { get { return _siValue; } }
        public DistanceUnit Unit { get { return _unit; } }

        public Distance ConvertTo(DistanceUnit unit)
        {
            return new Distance(_siValue.ConvertTo(unit), unit);
        }

        public override string ToString()
        {
            return UnitMetadata.For(_unit).Format(Value);
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
            return _siValue.Equals(other._siValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Distance && Equals((Distance) obj);
        }

        public override int GetHashCode()
        {
            return _siValue.GetHashCode();
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
            return left._siValue < right._siValue;
        }

        public static bool operator >(Area left, Area right)
        {
            return left._siValue > right._siValue;
        }

        public static bool operator <=(Area left, Area right)
        {
            return left._siValue <= right._siValue;
        }

        public static bool operator >=(Area left, Area right)
        {
            return left._siValue >= right._siValue;
        }

        public static Area operator +(Area left, Area right)
        {
            return new Area(left._siValue + right._siValue);
        }

        public static Area operator -(Area left, Area right)
        {
            return new Area(left._siValue - right._siValue);
        }
    }
}