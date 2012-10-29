namespace Geo.Measure
{
    public static class AreaUnitExtensions
    {
        public class AreaConversion
        {
            private readonly double _value;

            public AreaConversion(double value)
            {
                _value = value;
            }

            public double To(AreaUnit unit)
            {
                return _value.ConvertTo(unit);
            }
        }

        public static double GetConversionFactor(this AreaUnit unit)
        {
            return UnitMetadata.For(unit).ConversionFactor;
        }

        public static double ConvertTo(this double metres, AreaUnit unit)
        {
            return metres / unit.GetConversionFactor();
        }

        public static AreaConversion ConvertFrom(this double value, AreaUnit unit)
        {
            return new AreaConversion(value * unit.GetConversionFactor());
        }
    }
}