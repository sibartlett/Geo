using Geo.Geometries;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkt;

public class WktWriterTests
{
    [Fact]
    public void Point()
    {
        var writer = new WktWriter();

        var xy = writer.Write(new Point(65.9, 0));
        Assert.Equal("POINT (0 65.9)", xy);

        var xyz = writer.Write(new Point(65.9, 0, 5));
        Assert.Equal("POINT Z (0 65.9 5)", xyz);

        var xym = writer.Write(new Point(new CoordinateM(65.9, 0, 5)));
        Assert.Equal("POINT M (0 65.9 5)", xym);

        var xyzm = writer.Write(new Point(65.9, 0, 4, 5));
        Assert.Equal("POINT ZM (0 65.9 4 5)", xyzm);

        var empty = writer.Write(Geo.Geometries.Point.Empty);
        Assert.Equal("POINT EMPTY", empty);
    }

    [Fact]
    public void LineString()
    {
        var writer = new WktWriter();

        var xy = writer.Write(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5)));
        Assert.Equal("LINESTRING (0 65.9, -34.5 9)", xy);

        var empty = writer.Write(Geo.Geometries.LineString.Empty);
        Assert.Equal("LINESTRING EMPTY", empty);
    }

    [Fact]
    public void LinearRing()
    {
        var writer = new WktWriter();

        var lineString = writer.Write(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(50, 0), new Coordinate(65.9, 0)));
        Assert.Equal("LINESTRING (0 65.9, -34.5 9, 0 50, 0 65.9)", lineString);

        var writer2 = new WktWriter(new WktWriterSettings { LinearRing = true });

        var linearRing = writer2.Write(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(50, 0), new Coordinate(65.9, 0)));
        Assert.Equal("LINEARRING (0 65.9, -34.5 9, 0 50, 0 65.9)", linearRing);

        var empty = writer2.Write(Geo.Geometries.LinearRing.Empty);
        Assert.Equal("LINEARRING EMPTY", empty);
    }

    [Fact]
    public void Polygon()
    {
        var writer = new WktWriter();

        var xy = writer.Write(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(40, -20), new Coordinate(65.9, 0))));
        Assert.Equal("POLYGON ((0 65.9, -34.5 9, -20 40, 0 65.9))", xy);

        var empty = writer.Write(Geo.Geometries.Polygon.Empty);
        Assert.Equal("POLYGON EMPTY", empty);
    }

    [Fact]
    public void Triangle()
    {
        var writer = new WktWriter();

        var polygon = writer.Write(new Triangle(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(40, -20), new Coordinate(65.9, 0))));
        Assert.Equal("POLYGON ((0 65.9, -34.5 9, -20 40, 0 65.9))", polygon);

        var writer2 = new WktWriter(new WktWriterSettings { Triangle = true });

        var triangle = writer2.Write(new Triangle(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(40, -20), new Coordinate(65.9, 0))));
        Assert.Equal("TRIANGLE ((0 65.9, -34.5 9, -20 40, 0 65.9))", triangle);

        var empty = writer2.Write(Geo.Geometries.Triangle.Empty);
        Assert.Equal("TRIANGLE EMPTY", empty);
    }

    [Fact]
    public void GeometryCollection()
    {
        var writer = new WktWriter();

        var brackets = writer.Write(new GeometryCollection(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20),
            new Point(65.9, 0)));
        Assert.Equal("GEOMETRYCOLLECTION (POINT (0 65.9), POINT (-34.5 9), POINT (-20 40), POINT (0 65.9))",
            brackets);

        var empty = writer.Write(new GeometryCollection());
        Assert.Equal("GEOMETRYCOLLECTION EMPTY", empty);
    }

    [Fact]
    public void MultiPoint()
    {
        var writer = new WktWriter();

        var brackets = writer.Write(new MultiPoint(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20),
            new Point(65.9, 0)));
        Assert.Equal("MULTIPOINT ((0 65.9), (-34.5 9), (-20 40), (0 65.9))", brackets);

        var empty = writer.Write(new MultiPoint());
        Assert.Equal("MULTIPOINT EMPTY", empty);
    }

    [Fact]
    public void MultiLineString()
    {
        var writer = new WktWriter();

        var one = writer.Write(new MultiLineString(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
            new Coordinate(40, -20), new Coordinate(65.9, 0))));
        Assert.Equal("MULTILINESTRING ((0 65.9, -34.5 9, -20 40, 0 65.9))", one);


        var two = writer.Write(new MultiLineString(
            new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0)),
            new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0))));
        Assert.Equal("MULTILINESTRING ((0 65.9, -34.5 9, -20 40, 0 65.9), (0 65.9, -34.5 9, -20 40, 0 65.9))", two);

        var empty = writer.Write(new MultiLineString());
        Assert.Equal("MULTILINESTRING EMPTY", empty);
    }

    [Fact]
    public void MultiPolygon()
    {
        var writer = new WktWriter();

        var one = writer.Write(new MultiPolygon(new Polygon(new LinearRing(new Coordinate(65.9, 0),
            new Coordinate(9, -34.5), new Coordinate(40, -20), new Coordinate(65.9, 0)))));
        Assert.Equal("MULTIPOLYGON (((0 65.9, -34.5 9, -20 40, 0 65.9)))", one);


        var two = writer.Write(new MultiPolygon(
            new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0))),
            new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0)))));
        Assert.Equal("MULTIPOLYGON (((0 65.9, -34.5 9, -20 40, 0 65.9)), ((0 65.9, -34.5 9, -20 40, 0 65.9)))", two);

        var empty = writer.Write(new MultiPolygon());
        Assert.Equal("MULTIPOLYGON EMPTY", empty);
    }
}