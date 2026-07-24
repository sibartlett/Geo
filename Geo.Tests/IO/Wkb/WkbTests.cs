using System;
using System.Linq;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.IO.Wkb;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkb;

public class WkbTests
{
    [Fact]
    public void Point()
    {
        Test("POINT EMPTY");
        Test("POINT (45.89 23.9)");
        Test("POINT Z (45.89 23.9 0.45)");
        Test("POINT M (45.89 23.9 34)");
        Test("POINT ZM (45.89 23.9 0.45 34)");
    }

    [Fact]
    public void LineString()
    {
        Test("LINESTRING EMPTY");
        Test("LINESTRING (45.89 23.9, 0 0)");
        Test("LINESTRING Z (45.89 23.9 0.45, 0 0 0.45)");
        Test("LINESTRING M (45.89 23.9 34, 0 0 34)");
        Test("LINESTRING ZM (45.89 23.9 0.45 34, 0 0 0.45 34)");
    }

    [Fact]
    public void Polygon()
    {
        Test("POLYGON EMPTY");
        Test("POLYGON ((0 0, 1 0, 0 1, 0 0))");
        Test("POLYGON Z ((0 0 2, 1 0 2, 0 1 2, 0 0 2))");
        Test("POLYGON M ((0 0 -1, 1 0 -1, 0 1 -1, 0 0 -1))");
        Test("POLYGON ZM ((0 0 2 -1, 1 0 2 -1, 0 1 2 -1, 0 0 2 -1))");
    }

    [Fact]
    public void Triangle()
    {
        Test("TRIANGLE EMPTY");
        Test("TRIANGLE ((0 0, 1 0, 0 1, 0 0))");
        Test("TRIANGLE Z ((0 0 2, 1 0 2, 0 1 2, 0 0 2))");
        Test("TRIANGLE M ((0 0 -1, 1 0 -1, 0 1 -1, 0 0 -1))");
        Test("TRIANGLE ZM ((0 0 2 -1, 1 0 2 -1, 0 1 2 -1, 0 0 2 -1))");
    }

    [Fact]
    public void MultiPoint()
    {
        Test("MULTIPOINT EMPTY");
        Test("MULTIPOINT ((45.89 23.9), (0 0))");
        Test("MULTIPOINT Z ((45.89 23.9 0.45), (0 0 0.45))");
        Test("MULTIPOINT M ((45.89 23.9 34), (0 0 34))");
        Test("MULTIPOINT ZM ((45.89 23.9 0.45 34), (0 0 0.45 34))");
    }

    [Fact]
    public void MultiLineString()
    {
        Test("MULTILINESTRING EMPTY");
        Test("MULTILINESTRING ((45.89 23.9, 0 0), (1 1, 2 2))");
        Test("MULTILINESTRING Z ((45.89 23.9 0.45, 0 0 0.45), (1 1 3, 2 2 3))");
        Test("MULTILINESTRING M ((45.89 23.9 34, 0 0 34), (1 1 5, 2 2 5))");
        Test("MULTILINESTRING ZM ((45.89 23.9 0.45 34, 0 0 0.45 34), (1 1 3 5, 2 2 3 5))");
    }

    [Fact]
    public void MultiPolygon()
    {
        Test("MULTIPOLYGON EMPTY");
        Test("MULTIPOLYGON (((0 0, 1 0, 0 1, 0 0)), ((10 10, 11 10, 10 11, 10 10)))");
        Test(
            "MULTIPOLYGON Z (((0 0 2, 1 0 2, 0 1 2, 0 0 2)), ((10 10 2, 11 10 2, 10 11 2, 10 10 2)))"
        );
        Test(
            "MULTIPOLYGON M (((0 0 -1, 1 0 -1, 0 1 -1, 0 0 -1)), ((10 10 -1, 11 10 -1, 10 11 -1, 10 10 -1)))"
        );
        Test(
            "MULTIPOLYGON ZM (((0 0 2 -1, 1 0 2 -1, 0 1 2 -1, 0 0 2 -1)), ((10 10 2 -1, 11 10 2 -1, 10 11 2 -1, 10 10 2 -1)))"
        );
    }

    [Fact]
    public void GeometryCollection()
    {
        Test("GEOMETRYCOLLECTION (LINESTRING EMPTY, POLYGON EMPTY)");
        Test("GEOMETRYCOLLECTION (LINESTRING (45.89 23.9, 0 0), POLYGON ((0 0, 1 0, 0 1, 0 0)))");
        Test(
            "GEOMETRYCOLLECTION (LINESTRING Z (45.89 23.9 0.45, 0 0 0.45), POLYGON Z ((0 0 2, 1 0 2, 0 1 2, 0 0 2)))"
        );
        Test(
            "GEOMETRYCOLLECTION (LINESTRING M (45.89 23.9 34, 0 0 34), POLYGON M ((0 0 -1, 1 0 -1, 0 1 -1, 0 0 -1)))"
        );
        Test(
            "GEOMETRYCOLLECTION (LINESTRING ZM (45.89 23.9 0.45 34, 0 0 0.45 34), POLYGON ZM ((0 0 2 -1, 1 0 2 -1, 0 1 2 -1, 0 0 2 -1)))"
        );
    }

    [Fact]
    public void Empty_point_round_trips_as_a_non_empty_wkb_record()
    {
        // WKB has no empty flag for a point, so an empty point is encoded as a 2D point
        // with NaN ordinates (never zero bytes) and read back as an empty point.
        var wkb = new WkbWriter().Write(new Point());

        Assert.NotEmpty(wkb);

        var result = new WkbReader().Read(wkb);

        Assert.NotNull(result);
        Assert.True(result!.IsEmpty);
        Assert.IsType<Point>(result);
    }

    [Fact]
    public void Empty_points_are_preserved_inside_a_multipoint()
    {
        var multiPoint = new MultiPoint(new Point(1, 2), new Point(), new Point(3, 4));

        var wkb = new WkbWriter().Write(multiPoint);
        var result = (MultiPoint)new WkbReader().Read(wkb)!;

        Assert.Equal(3, result.Geometries.Count);
        Assert.True(result.Geometries.ElementAt(1).IsEmpty);
        Assert.Equal(multiPoint, result);
    }

    [Fact]
    public void Empty_points_are_preserved_inside_a_geometry_collection()
    {
        var collection = new GeometryCollection(new Point(), new Point(5, 6));

        var wkb = new WkbWriter().Write(collection);
        var result = (GeometryCollection)new WkbReader().Read(wkb)!;

        Assert.Equal(2, result.Geometries.Count);
        Assert.True(result.Geometries.ElementAt(0).IsEmpty);
        Assert.Equal(collection, result);
    }

    [Fact]
    public void Read_null_bytes_throws_argument_null()
    {
        Assert.Throws<ArgumentNullException>(() => new WkbReader().Read((byte[])null));
    }

    [Fact]
    public void Read_null_stream_throws_argument_null()
    {
        Assert.Throws<ArgumentNullException>(() => new WkbReader().Read((System.IO.Stream)null));
    }

    [Fact]
    public void Read_empty_bytes_returns_null()
    {
        Assert.Null(new WkbReader().Read(new byte[0]));
    }

    [Fact]
    public void Read_unknown_geometry_type_throws_serialization()
    {
        // Little-endian byte order marker (0x01) followed by an unknown type code (99).
        var bytes = new byte[] { 0x01, 99, 0, 0, 0 };
        Assert.Throws<SerializationException>(() => new WkbReader().Read(bytes));
    }

    [Fact]
    public void Read_truncated_geometry_throws_serialization()
    {
        // A little-endian Point (type 1) header with no coordinate data following.
        var bytes = new byte[] { 0x01, 1, 0, 0, 0 };
        Assert.Throws<SerializationException>(() => new WkbReader().Read(bytes));
    }

    private void Test(string wkt)
    {
        var wktReader = new WktReader();
        var geometry = wktReader.Read(wkt);
        {
            var wkbWriter = new WkbWriter(new WkbWriterSettings { Triangle = true });
            var wkb = wkbWriter.Write(geometry);
            var wkbReader = new WkbReader();
            var geometry2 = wkbReader.Read(wkb);
            Assert.Equal(geometry, geometry2);
        }
        {
            var wkbWriter = new WkbWriter(
                new WkbWriterSettings { Encoding = WkbEncoding.BigEndian, Triangle = true }
            );
            var wkb = wkbWriter.Write(geometry);
            var wkbReader = new WkbReader();
            var geometry2 = wkbReader.Read(wkb);
            Assert.Equal(geometry, geometry2);
        }
    }
}
