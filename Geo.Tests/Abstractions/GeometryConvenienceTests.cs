using Geo.Geometries;
using Geo.IO.Wkb;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.Abstractions;

/// <summary>
/// The <see cref="Geo.Abstractions.Geometry" /> base class exposes serialization
/// shortcuts (ToWktString / ToWkbBinary / ToGeoJson) that every geometry inherits.
/// </summary>
public class GeometryConvenienceTests
{
    private static readonly Point Sample = new(45.89, 23.9);

    [Fact]
    public void ToWktString_round_trips_through_the_wkt_reader()
    {
        var wkt = Sample.ToWktString();

        Assert.StartsWith("POINT", wkt);
        Assert.Equal(Sample, new WktReader().Read(wkt));
    }

    [Fact]
    public void ToWktString_with_settings_round_trips()
    {
        var wkt = Sample.ToWktString(new WktWriterSettings());

        Assert.Equal(Sample, new WktReader().Read(wkt));
    }

    [Fact]
    public void ToWkbBinary_round_trips_through_the_wkb_reader()
    {
        var wkb = Sample.ToWkbBinary();

        Assert.NotEmpty(wkb);
        Assert.Equal(Sample, new WkbReader().Read(wkb));
    }

    [Fact]
    public void ToWkbBinary_with_big_endian_settings_round_trips()
    {
        var wkb = Sample.ToWkbBinary(new WkbWriterSettings { Encoding = WkbEncoding.BigEndian });

        Assert.Equal(Sample, new WkbReader().Read(wkb));
    }

    [Fact]
    public void ToGeoJson_emits_a_point_document()
    {
        var geoJson = Sample.ToGeoJson();

        Assert.Contains("\"Point\"", geoJson);
        Assert.Contains("23.9", geoJson);
    }
}
