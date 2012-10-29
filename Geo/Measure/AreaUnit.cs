namespace Geo.Measure
{
    public enum AreaUnit
    {
        [Unit("m²", 1 * 1)]
        M = 0,
        [Unit("nm²", 1852 * 1852)]
        Nm = 1,
        [Unit("km²", 1000 * 1000)]
        Km = 2,
        [Unit("mi²", 1609.34 * 1609.34)]
        Mile = 3,
        [Unit("ft²", 0.3048 * 0.3048)]
        Ft = 4
    }
}