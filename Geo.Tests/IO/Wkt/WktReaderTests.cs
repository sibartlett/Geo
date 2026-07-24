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
    public void Whitespace_only_input_returns_null()
    {
        var reader = new WktReader();

        Assert.Null(reader.Read("   \t\r\n"));
    }

    [Theory]
    [InlineData("POINT 1 2")] // missing opening parenthesis
    [InlineData("POINT (1 $)")] // invalid character in the coordinate
    [InlineData("LINESTRING (1 2,)")] // trailing comma with no coordinate
    [InlineData("MULTIPOINT ((1 2)")] // token underflow: missing closing parenthesis
    [InlineData("POLYGON (")] // truncated immediately after the opening parenthesis
    public void Malformed_wkt_throws_serialization(string wkt)
    {
        var reader = new WktReader();

        Assert.Throws<SerializationException>(() => reader.Read(wkt));
    }

    [Fact]
    public void Truncated_geometry_throws_serialization()
    {
        var reader = new WktReader();

        // Runs out of tokens before the closing parenthesis; must surface as a
        // SerializationException rather than leaking an InvalidOperationException.
        Assert.Throws<SerializationException>(() => reader.Read("POINT (1 2"));
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
    public void Point_with_null_ordinate_placeholder_reads_as_measured()
    {
        var reader = new WktReader();

        // Without a dimension flag, an XYM coordinate is written positionally
        // with a NaN placeholder in the Z slot; it must read back as XYM.
        var xym = reader.Read("POINT (0.0 65.9 NaN 5)");
        Assert.Equal(new Point(new CoordinateM(65.9, 0, 5)), xym);
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
            new LinearRing(
                new Coordinate(65.9, 0),
                new Coordinate(9, -34.5),
                new Coordinate(65.9, 5),
                new Coordinate(65.9, 0)
            ),
            xy
        );

        var empty = reader.Read("LINEARRING ZM EMPTY");
        Assert.Equal(new LinearRing(), empty);
    }

    [Fact]
    public void Polygon()
    {
        var reader = new WktReader();

        var xy = reader.Read("POLYGON ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new Polygon(
                new LinearRing(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            ),
            xy
        );

        var empty = reader.Read("POLYGON ZM EMPTY");
        Assert.Equal(Geo.Geometries.Polygon.Empty, empty);
    }

    [Fact]
    public void Polygon_with_holes()
    {
        // Regression test for issue #30: a POLYGON with one or more interior
        // rings (holes) used to throw "Invalid WKT String".
        var reader = new WktReader();

        var polygon = (Polygon)
            reader.Read(
                "POLYGON ("
                    + "(-87.93939 41.98667, -87.93933 41.98729, -87.93906 41.98911, -87.93939 41.98667), "
                    + "(-87.83493 41.98116, -87.83434 41.98115, -87.83433 41.98082, -87.83493 41.98116), "
                    + "(-87.69615 41.69896, -87.69589 41.69161, -87.69574 41.69162, -87.69615 41.69896))"
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
    public void Triangle()
    {
        var reader = new WktReader();

        var xy = reader.Read("TRIANGLE ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new Triangle(
                new LinearRing(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            ),
            xy
        );

        var empty = reader.Read("Triangle ZM EMPTY");
        Assert.Equal(Geo.Geometries.Triangle.Empty, empty);
    }

    [Fact]
    public void GeometryCollection()
    {
        var reader = new WktReader();

        var points = reader.Read(
            "GEOMETRYCOLLECTION (POINT (0.0 65.9), POINT (-34.5 9), POINT  (-20 40), POINT (0 65.9))"
        );
        Assert.Equal(
            new GeometryCollection(
                new Point(65.9, 0),
                new Point(9, -34.5),
                new Point(40, -20),
                new Point(65.9, 0)
            ),
            points
        );

        var withEmptyMember = reader.Read("GEOMETRYCOLLECTION (POINT EMPTY, POINT (0.0 65.9))");
        Assert.Equal(new GeometryCollection(new Point(), new Point(65.9, 0)), withEmptyMember);
        Assert.True(((GeometryCollection)withEmptyMember!).Geometries[0].IsEmpty);

        var empty = reader.Read("GEOMETRYCOLLECTION ZM EMPTY");
        Assert.Equal(new GeometryCollection(), empty);
    }

    [Fact]
    public void MultiPoint()
    {
        var reader = new WktReader();

        var none = reader.Read("MULTIPOINT (0.0 65.9, -34.5 9, -20 40, 0 65.9)");
        Assert.Equal(
            new MultiPoint(
                new Point(65.9, 0),
                new Point(9, -34.5),
                new Point(40, -20),
                new Point(65.9, 0)
            ),
            none
        );

        var brackets = reader.Read("MULTIPOINT (EMPTY, (0.0 65.9), (-34.5 9), (-20 40), (0 65.9))");
        Assert.Equal(
            new MultiPoint(
                new Point(),
                new Point(65.9, 0),
                new Point(9, -34.5),
                new Point(40, -20),
                new Point(65.9, 0)
            ),
            brackets
        );

        var empty = reader.Read("MULTIPOINT ZM EMPTY");
        Assert.Equal(new MultiPoint(), empty);
    }

    [Fact]
    public void MultiLineString()
    {
        var reader = new WktReader();

        var one = reader.Read("MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9))");
        Assert.Equal(
            new MultiLineString(
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            ),
            one
        );

        var two = reader.Read(
            "MULTILINESTRING ((0.0 65.9, -34.5 9, -20 40, 0 65.9), (0.0 65.9, -34.5 9, -20 40, 0 65.9))"
        );
        Assert.Equal(
            new MultiLineString(
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                ),
                new LineString(
                    new Coordinate(65.9, 0),
                    new Coordinate(9, -34.5),
                    new Coordinate(40, -20),
                    new Coordinate(65.9, 0)
                )
            ),
            two
        );

        var empty = reader.Read("MULTILINESTRING ZM EMPTY");
        Assert.Equal(new MultiLineString(), empty);
    }

    [Fact]
    public void MultiPolygon()
    {
        var reader = new WktReader();

        var one = reader.Read("MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)))");
        Assert.Equal(
            new MultiPolygon(
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                )
            ),
            one
        );

        var two = reader.Read(
            "MULTIPOLYGON (((0.0 65.9, -34.5 9, -20 40, 0 65.9)),((0.0 65.9, -34.5 9, -20 40, 0 65.9)))"
        );
        Assert.Equal(
            new MultiPolygon(
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                ),
                new Polygon(
                    new LinearRing(
                        new Coordinate(65.9, 0),
                        new Coordinate(9, -34.5),
                        new Coordinate(40, -20),
                        new Coordinate(65.9, 0)
                    )
                )
            ),
            two
        );

        var empty = reader.Read("MULTIPOLYGON ZM EMPTY");
        Assert.Equal(new MultiPolygon(), empty);
    }
}
