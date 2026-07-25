using System;
using System.IO;
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

    [Fact]
    public void Sequence_mixing_2d_and_3d_coordinates_round_trips()
    {
        // Regression: the type code is derived from the geometry (Is3D is an "any
        // coordinate" test) while the ordinates used to be written per coordinate, so
        // the 2D coordinate wrote 8 bytes fewer than the Z type code promised and the
        // reader ran off the end of the geometry.
        TestRoundTrip(new LineString(new CoordinateZ(0, 0, 10), new Coordinate(1, 1)));
        TestRoundTrip(new LineString(new Coordinate(0, 0), new CoordinateZ(1, 1, 20)));
    }

    [Fact]
    public void Sequence_mixing_measured_and_unmeasured_coordinates_round_trips()
    {
        TestRoundTrip(new LineString(new CoordinateM(0, 0, 5), new Coordinate(1, 1)));
        TestRoundTrip(new LineString(new CoordinateZ(0, 0, 10), new CoordinateM(1, 1, 5)));
        TestRoundTrip(new LineString(new CoordinateZM(0, 0, 1, 2), new CoordinateZ(1, 1, 3)));
    }

    [Fact]
    public void Polygon_takes_its_dimensions_from_its_holes_as_well_as_its_shell()
    {
        // Regression: Polygon.Is3D reports only the shell, so a polygon whose
        // elevations live in a hole declared itself two-dimensional and then wrote
        // three ordinates per hole coordinate. That misaligns the reader rather than
        // merely truncating it, because the ring boundaries land in the wrong place.
        var flat = new LinearRing(
            new Coordinate(0, 0),
            new Coordinate(0, 3),
            new Coordinate(3, 3),
            new Coordinate(0, 0)
        );
        var raised = new LinearRing(
            new CoordinateZ(1, 1, 7),
            new CoordinateZ(1, 2, 7),
            new CoordinateZ(2, 2, 7),
            new CoordinateZ(1, 1, 7)
        );

        TestRoundTrip(new Polygon(flat, raised));
        TestRoundTrip(new Polygon(raised, flat));
        TestRoundTrip(new MultiPolygon(new Polygon(flat), new Polygon(raised)));
    }

    [Fact]
    public void Mixed_dimension_collection_round_trips()
    {
        TestRoundTrip(new GeometryCollection(new Point(0, 0, 5), new Point(1, 1)));
        TestRoundTrip(new MultiPoint(new Point(0, 0, 5), new Point(1, 1)));
    }

    [Fact]
    public void Padded_coordinate_declares_the_dimensions_of_its_type_code()
    {
        // The geometry must be exactly as long as its type code says: a Z line of two
        // points is 1 + 4 + 4 + 2 * 24 bytes, whichever coordinates carry elevations.
        var mixed = new WkbWriter().Write(
            new LineString(new CoordinateZ(0, 0, 10), new Coordinate(1, 1))
        );
        var uniform = new WkbWriter().Write(
            new LineString(new CoordinateZ(0, 0, 10), new CoordinateZ(1, 1, 20))
        );

        Assert.Equal(1002u, BitConverter.ToUInt32(mixed, 1));
        Assert.Equal(57, mixed.Length);
        Assert.Equal(uniform.Length, mixed.Length);
    }

    private void TestRoundTrip(Geo.Abstractions.Interfaces.IGeometry geometry)
    {
        var wkb = new WkbWriter(new WkbWriterSettings { Triangle = true }).Write(geometry);
        Assert.Equal(geometry, new WkbReader().Read(wkb));

        var bigEndian = new WkbWriter(
            new WkbWriterSettings { Encoding = WkbEncoding.BigEndian, Triangle = true }
        ).Write(geometry);
        Assert.Equal(geometry, new WkbReader().Read(bigEndian));
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

    [Fact]
    public void Read_from_a_non_seekable_stream()
    {
        // A network, pipe or compression stream cannot seek. Reading WKB from one must
        // return the geometry, not silently decide the stream holds no data.
        var expected = new Point(23.9, 45.89);
        var bytes = new WkbWriter().Write(expected);

        var geometry = new WkbReader().Read(new NonSeekableStream(bytes));

        Assert.Equal(expected, geometry);
    }

    [Fact]
    public void Read_from_an_empty_non_seekable_stream_returns_null()
    {
        Assert.Null(new WkbReader().Read(new NonSeekableStream(new byte[0])));
    }

    [Fact]
    public void Read_from_a_stream_that_returns_short_reads()
    {
        // A stream is free to satisfy a read with fewer bytes than were asked for
        // without being at its end; the reader must keep reading rather than treat the
        // short read as a truncated geometry.
        var expected = new Point(23.9, 45.89);
        var bytes = new WkbWriter().Write(expected);

        var geometry = new WkbReader().Read(new NonSeekableStream(bytes, 3));

        Assert.Equal(expected, geometry);
    }

    // A read-only stream that reports CanSeek == false, and optionally caps how many
    // bytes a single read will return.
    private sealed class NonSeekableStream : Stream
    {
        private readonly MemoryStream _inner;
        private readonly int _maxRead;

        public NonSeekableStream(byte[] data, int maxRead = int.MaxValue)
        {
            _inner = new MemoryStream(data);
            _maxRead = maxRead;
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            _inner.Read(buffer, offset, Math.Min(count, _maxRead));

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new NotSupportedException();
    }
}
