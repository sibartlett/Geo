using System;
using Geo.Interfaces;

namespace Geo.Measure
{
    public struct Speed : IMeasure, IEquatable<Speed>, IComparable<Speed>
    {
        private readonly double _siValue;
        private readonly SpeedUnit _unit;

        public Speed(double metresPerSecond)
        {
            _siValue = metresPerSecond;
            _unit = SpeedUnit.Ms;
        }

        public Speed(double value, SpeedUnit unit)
        {
            _siValue = value.ConvertFrom(unit).To(SpeedUnit.Ms);
            _unit = unit;
        }

        public Speed(double metres, TimeSpan timeSpan)
        {
            _unit = SpeedUnit.Ms;
            if (Math.Abs(metres - 0d) < double.Epsilon || timeSpan == default(TimeSpan) || Math.Abs(timeSpan.TotalSeconds - 0) < double.Epsilon)
                _siValue = 0d;
            else
                _siValue = metres / timeSpan.TotalSeconds;
        }

        //public Speed(Distance distance, TimeSpan timeSpan) : this(distance.Value, timeSpan)
        //{
        //}

        public double Value { get { return _siValue.ConvertTo(_unit); } }
        public double SiValue { get { return _siValue; } }
        public SpeedUnit Unit { get { return _unit; } }

        public Speed ConvertTo(SpeedUnit unit)
        {
            return new Speed(_siValue.ConvertTo(unit), unit);
        }

        public override string ToString()
        {
            return UnitMetadata.For(_unit).Format(Value);
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
            return _siValue.Equals(other._siValue);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is Speed && Equals((Speed) obj);
        }

        public override int GetHashCode()
        {
            return _siValue.GetHashCode();
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
            return left._siValue < right._siValue;
        }

        public static bool operator >(Speed left, Speed right)
        {
            return left._siValue > right._siValue;
        }

        public static bool operator <=(Speed left, Speed right)
        {
            return left._siValue <= right._siValue;
        }

        public static bool operator >=(Speed left, Speed right)
        {
            return left._siValue >= right._siValue;
        }

        public static Speed operator +(Speed left, Speed right)
        {
            return new Speed(left._siValue + right._siValue);
        }

        public static Speed operator -(Speed left, Speed right)
        {
            return new Speed(left._siValue - right._siValue);
        }
    }
}
