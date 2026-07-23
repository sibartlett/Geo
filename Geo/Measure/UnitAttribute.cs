using System;

namespace Geo.Measure;

[AttributeUsage(AttributeTargets.Field)]
internal sealed class UnitAttribute : Attribute
{
    private readonly string _symbol;

    public UnitAttribute(string symbol, double conversionFactor)
    {
        _symbol = symbol;
        ConversionFactor = conversionFactor;
    }

    public double ConversionFactor { get; }

    public double ConvertTo(double siUnit)
    {
        return siUnit / ConversionFactor;
    }

    public double ConvertFrom(double units)
    {
        return units * ConversionFactor;
    }

    public string Format(double value)
    {
        return string.Format("{0} {1}", value, _symbol);
    }
}
