using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using Geo.Geometries;
using Geo.IO.GeoJson;
using Xunit;

namespace Geo.Tests.IO.GeoJson;

public class GeoJsonReaderWriterTests
{
    // ---- 1. 3D (Z) and measured (ZM) coordinates ---------------------------

    [Fact]
    public void Writes_and_reads_3d_point()
    {
        var reader = new GeoJsonReader();
        var point = new Point(1, 2, 3); // latitude, longitude, elevation

        Assert.Equal("{\"type\":\"Point\",\"coordinates\":[2,1,3]}", point.ToGeoJson());
        Assert.Equal(point, reader.Read(point.ToGeoJson()));
    }

    [Fact]
    public void Writes_and_reads_measured_3d_point()
    {
        var reader = new GeoJsonReader();
        var point = new Point(1, 2, 3, 4); // latitude, longitude, elevation, measure

        Assert.Equal("{\"type\":\"Point\",\"coordinates\":[2,1,3,4]}", point.ToGeoJson());
        Assert.Equal(point, reader.Read(point.ToGeoJson()));
    }

    [Fact]
    public void Writes_and_reads_3d_linestring()
    {
        var reader = new GeoJsonReader();
        var line = new LineString(new CoordinateZ(0, 0, 10), new CoordinateZ(1, 1, 20));

        Assert.Equal(
            "{\"type\":\"LineString\",\"coordinates\":[[0,0,10],[1,1,20]]}",
            line.ToGeoJson()
        );
        Assert.Equal(line, reader.Read(line.ToGeoJson()));
    }

    // ---- 2. Feature properties with nested objects/arrays ------------------

    [Fact]
    public void Reads_feature_with_nested_object_and_array_properties()
    {
        var reader = new GeoJsonReader();
        var feature = (Feature)
            reader.Read(
                "{\"type\":\"Feature\",\"geometry\":{\"type\":\"Point\",\"coordinates\":[0,0]},"
                    + "\"properties\":{\"tags\":[\"a\",\"b\"],\"meta\":{\"k\":1}}}"
            );

        var tags = (object[])feature.Properties["tags"]!;
        Assert.Equal(new object[] { "a", "b" }, tags);

        // Nested objects are recursively sanitised into dictionaries whose leaf
        // values are the underlying JSON values (regression: they used to be
        // wrapped in key/value pairs).
        var meta = (Dictionary<string, object>)feature.Properties["meta"]!;
        Assert.Equal(1L, meta["k"]);
    }

    // ---- 3. Polygon-with-holes writing -------------------------------------

    [Fact]
    public void Writes_polygon_with_holes()
    {
        var reader = new GeoJsonReader();
        var polygon = new Polygon(
            new LinearRing(
                new Coordinate(0, 0),
                new Coordinate(0, 3),
                new Coordinate(3, 3),
                new Coordinate(0, 0)
            ),
            new LinearRing(
                new Coordinate(1, 1),
                new Coordinate(1, 2),
                new Coordinate(2, 2),
                new Coordinate(1, 1)
            )
        );

        var json = polygon.ToGeoJson();
        var read = (Polygon)reader.Read(json);

        Assert.Equal(polygon, read);
        Assert.Single(read.Holes);
    }

    // ---- 4. Circle conversion & unsupported-geometry error -----------------

    [Fact]
    public void Converts_circle_to_polygon_when_enabled()
    {
        var writer = new GeoJsonWriter(
            new GeoJsonWriterSettings { ConvertCirclesToRegularPolygons = true, CircleSides = 4 }
        );

        var json = writer.Write(new Circle(0, 0, 1000));

        Assert.StartsWith("{\"type\":\"Polygon\"", json);

        var polygon = (Polygon)new GeoJsonReader().Read(json);
        Assert.Equal(5, polygon.Shell!.Coordinates.Count); // 4 sides + closing point
    }

    [Fact]
    public void Writing_circle_without_conversion_throws_serialization()
    {
        Assert.Throws<SerializationException>(() =>
            new GeoJsonWriter().Write(new Circle(0, 0, 1000))
        );
    }

    // ---- 5. Malformed non-Point geometries fail ----------------------------

    [Theory]
    [InlineData("{\"type\":\"LineString\",\"coordinates\":[1,2]}")] // ordinates not arrays
    [InlineData("{\"type\":\"LineString\",\"coordinates\":[[1]]}")] // too few ordinates
    [InlineData("{\"type\":\"Polygon\",\"coordinates\":[1]}")]
    [InlineData("{\"type\":\"MultiPoint\",\"coordinates\":[1]}")]
    [InlineData("{\"type\":\"MultiLineString\",\"coordinates\":[1]}")]
    [InlineData("{\"type\":\"MultiPolygon\",\"coordinates\":[1]}")]
    [InlineData(
        "{\"type\":\"GeometryCollection\",\"geometries\":[{\"type\":\"Point\",\"coordinates\":[1]}]}"
    )]
    public void Read_malformed_geometry_throws_serialization(string json)
    {
        Assert.Throws<SerializationException>(() => new GeoJsonReader().Read(json));
    }

    // ---- 6. Static facade & stream reading ---------------------------------

    [Fact]
    public void Static_facade_serializes_and_deserializes_a_string()
    {
        var json = Geo.IO.GeoJson.GeoJson.Serialize(new Point(0, 0));

        Assert.Equal("{\"type\":\"Point\",\"coordinates\":[0,0]}", json);
        Assert.Equal(new Point(0, 0), Geo.IO.GeoJson.GeoJson.DeSerialize(json));
    }

    [Fact]
    public void Static_facade_deserializes_from_a_stream()
    {
        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes("{\"type\":\"Point\",\"coordinates\":[0,0]}")
        );

        Assert.Equal(new Point(0, 0), Geo.IO.GeoJson.GeoJson.DeSerialize(stream));
    }

    [Fact]
    public void Reader_reads_from_a_stream()
    {
        using var stream = new MemoryStream(
            Encoding.UTF8.GetBytes("{\"type\":\"Point\",\"coordinates\":[0,0]}")
        );

        Assert.Equal(new Point(0, 0), new GeoJsonReader().Read(stream));
    }

    // ---- 7. Write(object) dispatch, constructors & settings ----------------

    [Fact]
    public void Write_object_dispatches_by_runtime_type()
    {
        var writer = new GeoJsonWriter();
        var feature = new Feature(new Point(0, 0));
        var collection = new FeatureCollection(feature);

        Assert.Equal(writer.Write(feature), writer.Write((object)feature));
        Assert.Equal(writer.Write(collection), writer.Write((object)collection));
    }

    [Fact]
    public void Write_object_of_unsupported_type_throws_serialization()
    {
        Assert.Throws<SerializationException>(() =>
            new GeoJsonWriter().Write((object)"not a geometry")
        );
    }

    [Fact]
    public void Empty_feature_collection_has_no_features()
    {
        Assert.Empty(new FeatureCollection().Features);
    }

    [Fact]
    public void Nts_compatible_settings_are_available()
    {
        Assert.NotNull(GeoJsonWriterSettings.NtsCompatible);
    }
}
