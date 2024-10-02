using System;
using System.IO;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkt;

public class WktReaderTests
{
    [Fact]
    public void Invalid_geometry_type()
    {
        var reader = new WktReader();

        Assert.Throws<SerializationException>(() => reader.Read("SOMETHING EMPTY"));
    }

    [Fact]
    public void Null_input_string_throws_argument_exception()
    {
        var reader = new WktReader();

        Assert.Throws<ArgumentNullException>(() => reader.Read((string)null));
    }

    [Fact]
    public void Null_input_stream_throws_argument_exception()
    {
        var reader = new WktReader();

        Assert.Throws<ArgumentNullException>(() => reader.Read((Stream)null));
    }

    [Fact]
    public void Null()
    {
        var reader = new WktReader();

        var nothing = reader.Read("");
        Assert.Null(nothing);
    }

    [Fact]
    public void ExponentialNumber()
    {
        var reader = new WktReader();

        var xyWithE = reader.Read("POINT (5.5980439826435563E-06 -71.4920233210601)");
        Assert.Equal(new Point(-71.4920233210601, 5.5980439826435563E-06), xyWithE);
    }

    [Fact]
    public void Point()
    {
        var reader = new WktReader();

        var xy = reader.Read("POINT (0.0 65.9)");
        Assert.Equal(new Point(65.9, 0), xy);

        var xyz = reader.Read("POINT Z (0.0 65.9 5)");
        Assert.Equal(new Point(65.9, 0, 5), xyz);

        var xyz2 = reader.Read("POINT (0.0 65.9 5)");
        Assert.Equal(new Point(65.9, 0, 5), xyz2);

        var xym = reader.Read("POINT M (0.0 65.9 5)");
        Assert.Equal(new Point(new CoordinateM(65.9, 0, 5)), xym);

        var xyzm = reader.Read("POINT ZM (0.0 65.9 4 5)");
        Assert.Equal(new Point(65.9, 0, 4, 5), xyzm);

        var xyzm2 = reader.Read("POINT (0.0 65.9 4 5)");
        Assert.Equal(new Point(65.9, 0, 4, 5), xyzm2);

        var empty = reader.Read("POINT ZM EMPTY");
        Assert.Equal(Geo.Geometries.Point.Empty, empty);
    }

    [Fact]
    public void LineString()
    {
        var reader = new WktReader();

        var xy = reader.Read("LINESTRING (0.0 65.9, -34.5 9)");
        Assert.Equal(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5)), xy);

        var empty = reader.Read("LINESTRING ZM EMPTY");
        Assert.Equal(new LineString(), empty);
    }

    [Fact]
    public void LinearRing()
    {
        var reader = new WktReader();

        var xy = reader.Read("LINEARRING (0.0 65.9, -34.5 9, 5.0 65.9, 0.0 65.9)");
        Assert.Equal(
            new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(65.9, 5),
                new Coordinate(65.9, 0)), xy);

        var empty = reader.Read("LINEARRING ZM EMPTY");
        Assert.Equal(new LinearRing(), empty);
    }

    [Fact]
    public void Polygon()
    {
        var reader = new WktReader();

        var xy = reader.Read("POLYGON ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0))), xy);

        var empty = reader.Read("POLYGON ZM EMPTY");
        Assert.Equal(Geo.Geometries.Polygon.Empty, empty);
    }

    [Fact]
    public void Triangle()
    {
        var reader = new WktReader();

        var xy = reader.Read("TRIANGLE ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new Triangle(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                new Coordinate(65.9, 0))), xy);

        var empty = reader.Read("Triangle ZM EMPTY");
        Assert.Equal(Geo.Geometries.Triangle.Empty, empty);
    }

    [Fact]
    public void GeometryCollection()
    {
        var reader = new WktReader();

        var points =
            reader.Read("GEOMETRYCOLLECTION (POINT (0.0 65.9), POINT (-34.5 9), POINT  (-20 40), POINT (0 65.9))");
        Assert.Equal(
            new GeometryCollection(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)),
            points);

        var empty = reader.Read("GEOMETRYCOLLECTION ZM EMPTY");
        Assert.Equal(new GeometryCollection(), empty);
    }

    [Fact]
    public void MultiPoint()
    {
        var reader = new WktReader();

        var none = reader.Read("MULTIPOINT (0.0 65.9, -34.5 9, -20 40, 0 65.9)");
        Assert.Equal(new MultiPoint(new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20), new Point(65.9, 0)),
            none);

        var brackets = reader.Read("MULTIPOINT (EMPTY, (0.0 65.9), (-34.5 9), (-20 40), (0 65.9))");
        Assert.Equal(
            new MultiPoint(new Point(), new Point(65.9, 0), new Point(9, -34.5), new Point(40, -20),
                new Point(65.9, 0)), brackets);

        var empty = reader.Read("MULTIPOINT ZM EMPTY");
        Assert.Equal(new MultiPoint(), empty);
    }

    [Fact]
    public void MultiLineString()
    {
        var reader = new WktReader();

        var one = reader.Read("MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new MultiLineString(new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
                new Coordinate(40, -20), new Coordinate(65.9, 0))), one);


        var two = reader.Read(
            "MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9), (0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new MultiLineString(
                new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                    new Coordinate(65.9, 0)),
                new LineString(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                    new Coordinate(65.9, 0))), two);

        var empty = reader.Read("MULTILINESTRING ZM EMPTY");
        Assert.Equal(new MultiLineString(), empty);
    }

    [Fact]
    public void MultiPolygon()
    {
        var reader = new WktReader();

        var one = reader.Read("MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)))");
        Assert.Equal(
            new MultiPolygon(new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5),
                new Coordinate(40, -20), new Coordinate(65.9, 0)))), one);


        var two = reader.Read(
            "MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)),((0.0 65.9, -34.5 9, -20 40, 0 65.9)))");
        Assert.Equal(
            new MultiPolygon(
                new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                    new Coordinate(65.9, 0))),
                new Polygon(new LinearRing(new Coordinate(65.9, 0), new Coordinate(9, -34.5), new Coordinate(40, -20),
                    new Coordinate(65.9, 0)))), two);

        var empty = reader.Read("MULTIPOLYGON ZM EMPTY");
        Assert.Equal(new MultiPolygon(), empty);
    }
}