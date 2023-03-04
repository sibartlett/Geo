using System.Collections.Generic;
using System.Linq;
using Geo.Geometries;
using Geo.IO.GeoJson;
using NUnit.Framework;

namespace Geo.Tests.IO.GeoJson;

[TestFixture]
public class GeoJsonTests
{
    [Test]
    public void Point()
    {
        var reader = new GeoJsonReader();
        var geo = new Point(0, 0);
        Assert.AreEqual(@"{""type"":""Point"",""coordinates"":[0,0]}", geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void LineString()
    {
        var reader = new GeoJsonReader();
        var geo = new LineString(new Coordinate(0, 0), new Coordinate(1, 1));
        Assert.AreEqual(@"{""type"":""LineString"",""coordinates"":[[0,0],[1,1]]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void Polygon()
    {
        var reader = new GeoJsonReader();
        var geo =
            new Polygon(new LinearRing(new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 0),
                new Coordinate(0, 0)));
        Assert.AreEqual(@"{""type"":""Polygon"",""coordinates"":[[[0,0],[1,1],[0,2],[0,0]]]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void MultiPoint()
    {
        var reader = new GeoJsonReader();
        var geo = new MultiPoint(new Point(0, 0));
        Assert.AreEqual(@"{""type"":""MultiPoint"",""coordinates"":[[0,0]]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void MultiLineString()
    {
        var reader = new GeoJsonReader();
        var geo = new MultiLineString(new LineString(new Coordinate(0, 0), new Coordinate(1, 1)));
        Assert.AreEqual(@"{""type"":""MultiLineString"",""coordinates"":[[[0,0],[1,1]]]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void MultiPolygon()
    {
        var reader = new GeoJsonReader();
        var geo =
            new MultiPolygon(
                new Polygon(new LinearRing(new Coordinate(0, 0), new Coordinate(1, 1), new Coordinate(2, 0),
                    new Coordinate(0, 0))));
        Assert.AreEqual(@"{""type"":""MultiPolygon"",""coordinates"":[[[[0,0],[1,1],[0,2],[0,0]]]]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void GeometryCollection()
    {
        var reader = new GeoJsonReader();
        var geo = new GeometryCollection(new Point(0, 0), new Point(1, 0));
        Assert.AreEqual(
            @"{""type"":""GeometryCollection"",""geometries"":[{""type"":""Point"",""coordinates"":[0,0]},{""type"":""Point"",""coordinates"":[0,1]}]}",
            geo.ToGeoJson());
        Assert.AreEqual(geo, reader.Read(geo.ToGeoJson()));
    }

    [Test]
    public void Feature()
    {
        var reader = new GeoJsonReader();
        Assert.AreEqual(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}",
            new Feature(new Point(0, 0)) { Id = "test-id" }.ToGeoJson()
        );

        Assert.AreEqual(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}",
            new Feature(new Point(0, 0), new Dictionary<string, object>()) { Id = "test-id" }.ToGeoJson()
        );

        var feature = new Feature(new Point(0, 0), new Dictionary<string, object>
        {
            { "name", "test" }
        }) { Id = "test-id" };
        Assert.AreEqual(
            @"{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":{""name"":""test""},""id"":""test-id""}",
            feature.ToGeoJson()
        );

        var feature2 = (Feature)reader.Read(feature.ToGeoJson());
        Assert.AreEqual(feature.Id, feature2.Id);
        Assert.AreEqual(feature.Geometry, feature2.Geometry);
    }

    [Test]
    public void FeatureCollection()
    {
        var reader = new GeoJsonReader();
        var features = new FeatureCollection(new Feature(new Point(0, 0)) { Id = "test-id" });
        Assert.AreEqual(
            @"{""type"":""FeatureCollection"",""features"":[{""type"":""Feature"",""geometry"":{""type"":""Point"",""coordinates"":[0,0]},""properties"":null,""id"":""test-id""}]}",
            features.ToGeoJson()
        );

        var features2 = (FeatureCollection)reader.Read(features.ToGeoJson());
        Assert.AreEqual(features.Features.Single().Geometry, features2.Features.Single().Geometry);
    }
}