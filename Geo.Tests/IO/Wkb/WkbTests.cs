using System;
using System.Runtime.Serialization;
using Geo.IO.Wkb;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkb;

public class WkbTests
{
    [Fact]
    public void Point()
    {
        //Test("POINT EMPTY");
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
