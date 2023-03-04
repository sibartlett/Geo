namespace Geo.Measure;

public static class DistanceUnitExtensions
{
    public static double GetConversionFactor(this DistanceUnit unit)
    {
        return UnitMetadata.For(unit).ConversionFactor;
    }

    public static double ConvertTo(this double metres, DistanceUnit unit)
    {
        return metres / unit.GetConversionFactor();
    }

    public static DistanceConversion ConvertFrom(this double value, DistanceUnit unit)
    {
        return new DistanceConversion(value * unit.GetConversionFactor());
    }

    public class DistanceConversion
    {
        private readonly double _value;

        public DistanceConversion(double value)
        {
            _value = value;
        }

        public double To(DistanceUnit unit)
        {
            return _value.ConvertTo(unit);
        }
    }
}