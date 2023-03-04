using Geo.Abstractions.Interfaces;
using Geo.Geodesy;
using Geo.Geomagnetism;

namespace Geo;

public class GeoContext
{
    private static GeoContext _current;

    public GeoContext() : this(Spheroid.Default)
    {
    }

    public GeoContext(Spheroid spheroid)
    {
        Spheroid = spheroid;
        GeodeticCalculator = new SpheroidCalculator(spheroid);
        GeomagnetismCalculator = new GeomagnetismCalculator(spheroid);
        EqualityOptions = new SpatialEqualityOptions();
        LongitudeWrapping = false;
    }

    public static GeoContext Current
    {
        get => _current ?? (_current = new GeoContext());
        set => _current = value;
    }

    public Spheroid Spheroid { get; set; }
    public IGeodeticCalculator GeodeticCalculator { get; set; }
    public GeomagnetismCalculator GeomagnetismCalculator { get; set; }
    public SpatialEqualityOptions EqualityOptions { get; set; }
    public bool LongitudeWrapping { get; set; }
}