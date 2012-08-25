using System;

namespace Geo.Measure
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    internal sealed class UnitAttribute : Attribute
    {
        private readonly string _symbol;
        private readonly double _conversionFactor;

        public UnitAttribute(string symbol, double conversionFactor)
        {
            _symbol = symbol;
            _conversionFactor = conversionFactor;
        }

        public double ConversionFactor { get { return _conversionFactor; } }

        public double ConvertTo(double siUnit)
        {
            return siUnit / _conversionFactor;
        }

        public double ConvertFrom(double units)
        {
            return units * _conversionFactor;
        }

        public string Format(double value)
        {
            return string.Format("{0} {1}", value, _symbol);
        }
    }
}