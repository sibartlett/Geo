namespace Geo;

public class GeoJsonWriterSettings
{
    public GeoJsonWriterSettings()
    {
        ConvertCirclesToRegularPolygons = false;
        CircleSides = 36;
    }

    public bool ConvertCirclesToRegularPolygons { get; set; }
    public int CircleSides { get; set; }

    public static GeoJsonWriterSettings NtsCompatible => new();
}
