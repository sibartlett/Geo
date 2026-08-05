namespace Geo.Measure;

// The symbol and SI conversion factor of one unit.
//
// This was once a [Unit(...)] attribute on each enum member, read back through
// reflection by UnitMetadata. That could not survive a NativeAOT publish: the
// lookup went through Enum.GetValues(Type), which requires runtime code
// generation, and Type.GetField, whose target the trimmer is free to remove
// because nothing references it statically. The definitions now sit in a plain
// table in UnitMetadata, so the same data is resolved with no reflection at all.
internal sealed class UnitDefinition
{
    private readonly string _symbol;

    public UnitDefinition(string symbol, double conversionFactor)
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
