using Geo.Interfaces;

namespace Geo.Measure
{
    public struct Distance : IMeasure
    {
        private readonly double _siValue;
        private readonly DistanceUnit _unit;

        public Distance(double metres)
        {
            _siValue = metres;
            _unit = DistanceUnit.M;
        }

        public Distance(double value, DistanceUnit unit)
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

        //TODO
        //http://confluence.jetbrains.net/display/ReSharper/Compare+of+float+numbers+by+equality+operator

        public bool Equals(Distance other)
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
            return left._siValue < right._siValue;
        }

        public static bool operator >(Distance left, Distance right)
        {
            return left._siValue > right._siValue;
        }

        public static bool operator <=(Distance left, Distance right)
        {
            return left._siValue <= right._siValue;
        }

        public static bool operator >=(Distance left, Distance right)
        {
            return left._siValue >= right._siValue;
        }

        public static Distance operator +(Distance left, Distance right)
        {
            return new Distance(left._siValue + right._siValue);
        }

        public static Distance operator -(Distance left, Distance right)
        {
            return new Distance(left._siValue - right._siValue);
        }

        public static Area operator *(Distance left, Distance right)
        {
            return new Area(left._siValue * right._siValue);
        }
    }
}
