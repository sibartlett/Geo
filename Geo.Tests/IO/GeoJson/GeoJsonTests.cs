using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.IO.GeoJson;
using Xunit;

namespace Geo.Tests.IO.GeoJson;

public class GeoJsonTests
{
    [Fact]
    public void Point()
    {
        var reader = new GeoJsonReader();
        var geo = new Point(0, 0);
        Assert.Equal(@"{""type"":""Point"",""coordinates"":[0,0]}", geo.ToGeoJson());
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void Read_null_stream_throws_argument_null()
    {
        Assert.Throws<ArgumentNullException>(() => new GeoJsonReader().Read((Stream)null));
    }

    [Fact]
    public void Read_invalid_json_throws_serialization()
    {
        Assert.Throws<SerializationException>(() => new GeoJsonReader().Read("not valid json"));
    }

    [Fact]
    public void Read_json_without_a_recognised_type_throws_serialization()
    {
        Assert.Throws<SerializationException>(() =>
            new GeoJsonReader().Read(@"{""type"":""Nonsense""}")
        );
    }

    [Fact]
    public void TryRead_returns_false_for_invalid_json()
    {
        object result;
        Assert.False(new GeoJsonReader().TryRead("not valid json", out result));
        Assert.Null(result);
    }

    [Fact]
    public void TryRead_returns_false_for_valid_json_that_is_not_geojson()
    {
        object result;
        Assert.False(new GeoJsonReader().TryRead(@"{""foo"":1}", out result));
        Assert.Null(result);
    }

    [Theory]
    [InlineData(@"{""type"":""Point""}")] // no coordinates member
    [InlineData(@"{""type"":""Point"",""coordinates"":""nope""}")] // coordinates not an array
    [InlineData(@"{""type"":""Point"",""coordinates"":[1]}")] // too few ordinates
    [InlineData(@"{""type"":""GeometryCollection""}")] // no geometries member
    [InlineData(@"{""type"":""FeatureCollection""}")] // no features member
    [InlineData("[1,2,3]")] // a JSON array, not a GeoJSON object
    [InlineData("null")] // JSON null
    public void Read_malformed_geojson_throws_serialization(string json)
    {
        Assert.Throws<SerializationException>(() => new GeoJsonReader().Read(json));
    }

    [Fact]
    public void LineString()
    {
        var reader = new GeoJsonReader();
        var geo = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));
        Assert.Equal(@"{""type"":""LineString"",""coordinates"":[[0,0],[1,1]]}", geo.ToGeoJson());
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void Polygon()
    {
        var reader = new GeoJsonReader();
        var geo = new Polygon(
            new LinearRing(
                new Coordinate(0, 0),
                new Coordinate(1, 1),
                new Coordinate(2, 0),
                new Coordinate(0, 0)
            )
        );
        Assert.Equal(
            @"{""type"":""Polygon"",""coordinates"":[[[0,0],[1,1],[0,2],[0,0]]]}",
            geo.ToGeoJson()
        );
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void Polygon_with_holes()
    {
        // Regression test for issue #31: a Polygon with interior rings (holes)
        // used to throw "Invalid GeoJSON string". This is the GeoJSON
        // counterpart of the WKT hole-parsing issue #30.
        var reader = new GeoJsonReader();

        var polygon = (Polygon)
            reader.Read(
                @"{""type"":""Polygon"",""coordinates"":["
                    + @"[[-87.93939,41.98667],[-87.93933,41.98729],[-87.93906,41.98911],[-87.93939,41.98667]],"
                    + @"[[-87.83493,41.98116],[-87.83434,41.98115],[-87.83433,41.98082],[-87.83493,41.98116]],"
                    + @"[[-87.69615,41.69896],[-87.69589,41.69161],[-87.69574,41.69162],[-87.69615,41.69896]]]}"
            );

        Assert.Equal(
            new Polygon(
                new LinearRing(
                    new Coordinate(41.98667, -87.93939),
                    new Coordinate(41.98729, -87.93933),
                    new Coordinate(41.98911, -87.93906),
                    new Coordinate(41.98667, -87.93939)
                ),
                new LinearRing(
                    new Coordinate(41.98116, -87.83493),
                    new Coordinate(41.98115, -87.83434),
                    new Coordinate(41.98082, -87.83433),
                    new Coordinate(41.98116, -87.83493)
                ),
                new LinearRing(
                    new Coordinate(41.69896, -87.69615),
                    new Coordinate(41.69161, -87.69589),
                    new Coordinate(41.69162, -87.69574),
                    new Coordinate(41.69896, -87.69615)
                )
            ),
            polygon
        );

        Assert.Equal(2, polygon.Holes.Count);
    }

    [Fact]
    public void MultiPoint()
    {
        var reader = new GeoJsonReader();
        var geo = new MultiPoint(new Point(0, 0));
        Assert.Equal(@"{""type"":""MultiPoint"",""coordinates"":[[0,0]]}", geo.ToGeoJson());
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void MultiLineString()
    {
        var reader = new GeoJsonReader();
        var geo = new MultiLineString(new LineString(new Coordinate(0, 0), new Coordinate(1, 1)));
        Assert.Equal(
            @"{""type"":""MultiLineString"",""coordinates"":[[[0,0],[1,1]]]}",
            geo.ToGeoJson()
        );
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void MultiPolygon()
    {
        var reader = new GeoJsonReader();
        var geo = new MultiPolygon(
            new Polygon(
                new LinearRing(
                    new Coordinate(0, 0),
                    new Coordinate(1, 1),
                    new Coordinate(2, 0),
                    new Coordinate(0, 0)
                )
            )
        );
        Assert.Equal(
            @"{""type"":""MultiPolygon"",""coordinates"":[[[[0,0],[1,1],[0,2],[0,0]]]]}",
            geo.ToGeoJson()
        );
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void GeometryCollection()
    {
        var reader = new GeoJsonReader();
        var geo = new GeometryCollection(new Point(0, 0), new Point(1, 0));
        Assert.Equal(
            @"{""type"":""GeometryCollection"",""geometries"":[{""type"":""Point"",""coordinates"":[0,0]},{""type"":""Point"",""coordinates"":[0,1]}]}",
            geo.ToGeoJson()
        );
        Assert.Equal(geo, reader.Read(geo.ToGeoJson()));
    }

    [Fact]
    public void Feature()
    {
        var reader = new GeoJsonReader();
        Assert.Equal(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}",
            new Feature(new Point(0, 0)) { Id = "test-id" }.ToGeoJson()
        );

        Assert.Equal(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}",
            new Feature(new Point(0, 0), new Dictionary<string, object>())
            {
                Id = "test-id",
            }.ToGeoJson()
        );

        var feature = new Feature(
            new Point(0, 0),
            new Dictionary<string, object> { { "name", "test" } }
        )
        {
            Id = "test-id",
        };
        Assert.Equal(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":{""name"":""test""},""id"":""test-id""}",
            feature.ToGeoJson()
        );

        var feature2 = (Feature)reader.Read(feature.ToGeoJson());
        Assert.Equal(feature.Id, feature2.Id);
        Assert.Equal(feature.Geometry, feature2.Geometry);
    }

    [Fact]
    public void FeatureCollection()
    {
        var reader = new GeoJsonReader();
        var features = new FeatureCollection(new Feature(new Point(0, 0)) { Id = "test-id" });
        Assert.Equal(
            @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}]}",
            features.ToGeoJson()
        );

        var features2 = (FeatureCollection)reader.Read(features.ToGeoJson());
        Assert.Equal(features.Features.Single().Geometry, features2.Features.Single().Geometry);
    }
}
