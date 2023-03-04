namespace Geo.IO.Wkb;

public class WkbWriterSettings
{
    public WkbWriterSettings()
    {
        Encoding = WkbEncoding.LittleEndian;
        Triangle = false;
        MaxDimesions = 4;
        ConvertCirclesToRegularPolygons = false;
        CircleSides = 36;
    }

    public int MaxDimesions { get; set; }
    public WkbEncoding Encoding { get; set; }
    public bool Triangle { get; set; }
    public bool ConvertCirclesToRegularPolygons { get; set; }
    public int CircleSides { get; set; }

    public static WkbWriterSettings NtsCompatible => new();
}