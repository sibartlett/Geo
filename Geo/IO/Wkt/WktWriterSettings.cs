using System.Globalization;

namespace Geo.IO.Wkt;

public class WktWriterSettings
{
    public WktWriterSettings()
    {
        LinearRing = false;
        Triangle = false;
        DimensionFlag = true;
        //TODO: Review whether we need NullOrdinate setting
        NullOrdinate = double.NaN.ToString(CultureInfo.InvariantCulture);
        MaxDimesions = 4;
        ConvertCirclesToRegularPolygons = false;
        CircleSides = 36;
    }

    public int MaxDimesions { get; set; }
    public string NullOrdinate { get; set; }
    public bool DimensionFlag { get; set; }
    public bool LinearRing { get; set; }
    public bool Triangle { get; set; }
    public bool ConvertCirclesToRegularPolygons { get; set; }
    public int CircleSides { get; set; }

    public static WktWriterSettings NtsCompatible =>
        new() { DimensionFlag = false, LinearRing = true };
}
