using System.Collections.Generic;
using System.Linq;
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
