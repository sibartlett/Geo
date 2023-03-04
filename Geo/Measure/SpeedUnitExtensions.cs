namespace Geo.Measure;

public static class SpeedUnitExtensions
{
    public static double GetConversionFactor(this SpeedUnit unit)
    {
        return UnitMetadata.For(unit).ConversionFactor;
    }

    public static double ConvertTo(this double metres, SpeedUnit unit)
    {
        return metres / unit.GetConversionFactor();
    }

    public static SpeedConversion ConvertFrom(this double value, SpeedUnit unit)
    {
        return new SpeedConversion(value * unit.GetConversionFactor());
    }

    public class SpeedConversion
    {
        private readonly double _value;

        public SpeedConversion(double value)
        {
            _value = value;
        }

        public double To(SpeedUnit unit)
        {
            return _value.ConvertTo(unit);
        }
    }
}