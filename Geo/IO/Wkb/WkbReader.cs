#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO.Wkb;

public class WkbReader
{
    public IGeometry? Read(byte[] bytes)
    {
        if (bytes == null)
            throw new ArgumentNullException("bytes");

        using (var stream = new MemoryStream(bytes))
        {
            var geometry = Read(stream, out var bytesRead);

            // A byte array holds the geometry and nothing else, so anything left over
            // means it does not contain what it claims to. A stream is deliberately not
            // held to this standard: a caller may hand over one that carries the
            // geometry alongside whatever else it is already carrying.
            if (geometry != null && bytesRead != bytes.Length)
                throw new SerializationException(
                    (bytes.Length - bytesRead).ToString(CultureInfo.InvariantCulture)
                        + " bytes remain after the end of the WKB geometry."
                );

            return geometry;
        }
    }

    // WKB is read incrementally through a BinaryReader, which has no async API, so
    // the source stream is buffered into memory asynchronously up front and the
    // (CPU-bound) decoding then runs against that buffer.
    public async Task<IGeometry?> ReadAsync(
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        if (stream == null)
            throw new ArgumentNullException("stream");

        using (var buffer = new MemoryStream())
        {
            await stream.CopyToAsync(buffer, 16 * 1024, cancellationToken).ConfigureAwait(false);
            buffer.Position = 0;
            return Read(buffer);
        }
    }

    public IGeometry? Read(Stream stream)
    {
        return Read(stream, out _);
    }

    private IGeometry? Read(Stream stream, out long bytesRead)
    {
        if (stream == null)
            throw new ArgumentNullException("stream");

        bytesRead = 0;

        using (var reader = new WkbBinaryReader(stream))
        {
            if (!reader.HasData)
                return null;

            try
            {
                var geometry = ReadGeometry(reader);
                bytesRead = reader.BytesRead;
                return geometry;
            }
            catch (EndOfStreamException)
            {
                throw new SerializationException(
                    "End of stream reached before end of valid WKB geometry."
                );
            }
            // Malformed bytes reach the geometry and coordinate constructors as
            // impossible values - a latitude past the pole, a ring that does not close -
            // which reject them as bad arguments. The argument actually at fault is the
            // WKB, so these are reported like every other decoding failure rather than
            // surfacing as an argument exception naming a parameter the caller never
            // supplied.
            catch (ArgumentException ex)
            {
                throw new SerializationException("Invalid WKB geometry.", ex);
            }
        }
    }

    private Coordinate ReadCoordinate(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var x = reader.ReadDouble();
        var y = reader.ReadDouble();
        var z =
            dimensions == WkbDimensions.XYZ || dimensions == WkbDimensions.XYZM
                ? reader.ReadDouble()
                : double.NaN;
        var m =
            dimensions == WkbDimensions.XYM || dimensions == WkbDimensions.XYZM
                ? reader.ReadDouble()
                : double.NaN;

        if (!double.IsNaN(z) && !double.IsNaN(m))
            return new CoordinateZM(y, x, z, m);
        if (!double.IsNaN(z))
            return new CoordinateZ(y, x, z);
        if (!double.IsNaN(m))
            return new CoordinateM(y, x, m);
        return new Coordinate(y, x);
    }

    // Counts are read straight off the input, so they cannot be trusted to size a list:
    // a four-byte header would otherwise be able to demand an arbitrarily large
    // allocation before a single coordinate had been read, and a count above
    // int.MaxValue turns negative on the way to a capacity or a loop bound. Counts are
    // kept unsigned and the list grows into whatever actually arrives, so an
    // overstated count runs out of stream rather than out of memory.
    private const int MaxPreallocatedCount = 1024;

    private CoordinateSequence ReadCoordinates(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var pointCount = reader.ReadUInt32();

        var result = new List<Coordinate>((int)Math.Min(pointCount, MaxPreallocatedCount));
        for (var i = 0u; i < pointCount; i++)
            result.Add(ReadCoordinate(reader, dimensions));

        return new CoordinateSequence(result);
    }

    private IGeometry ReadGeometry(WkbBinaryReader reader)
    {
        reader.ReadAndSetEncoding();

        var type = reader.ReadUInt32();
        var dimensions = WkbDimensions.XY;
        if (type > 1000)
            dimensions = WkbDimensions.XYZ;
        if (type > 2000)
            dimensions = WkbDimensions.XYM;
        if (type > 3000)
            dimensions = WkbDimensions.XYZM;

        var geometryType = (WkbGeometryType)((int)type % 1000);

        switch (geometryType)
        {
            case WkbGeometryType.Point:
                return ReadPoint(reader, dimensions);
            case WkbGeometryType.LineString:
                return ReadLineString(reader, dimensions);
            case WkbGeometryType.Triangle:
                return ReadTriangle(reader, dimensions);
            case WkbGeometryType.Polygon:
                return ReadPolygon(reader, dimensions);
            case WkbGeometryType.MultiPoint:
                return ReadMultiPoint(reader, dimensions);
            case WkbGeometryType.MultiLineString:
                return ReadMultiLineString(reader, dimensions);
            case WkbGeometryType.MultiPolygon:
                return ReadMultiPolygon(reader, dimensions);
            case WkbGeometryType.GeometryCollection:
                return ReadGeometryCollection(reader);
            default:
                throw new SerializationException("Unknown geometry type.");
        }
    }

    private Point ReadPoint(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var coordinate = ReadCoordinate(reader, dimensions);

        // A point whose position ordinates are all NaN encodes POINT EMPTY, matching
        // NTS/GEOS/PostGIS. Return a fresh empty point rather than the shared singleton,
        // since Point.Coordinate is mutable.
        if (double.IsNaN(coordinate.Latitude) && double.IsNaN(coordinate.Longitude))
            return new Point();

        return new Point(coordinate);
    }

    private LineString ReadLineString(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        return new LineString(ReadCoordinates(reader, dimensions));
    }

    private Polygon ReadPolygon(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var rings = ReadPolygonInner(reader, dimensions);
        if (rings.Count == 0)
            return new Polygon();
        return new Polygon(rings.First(), rings.Skip(1));
    }

    private Polygon ReadTriangle(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var rings = ReadPolygonInner(reader, dimensions);
        if (rings.Count == 0)
            return new Triangle();
        return new Triangle(rings.First(), rings.Skip(1));
    }

    private List<LinearRing> ReadPolygonInner(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var result = new List<LinearRing>();
        var ringsCount = reader.ReadUInt32();
        for (var i = 0u; i < ringsCount; i++)
            result.Add(new LinearRing(ReadCoordinates(reader, dimensions)));
        return result;
    }

    private MultiPoint ReadMultiPoint(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var pointsCount = reader.ReadUInt32();
        var points = new List<Point>();
        for (var i = 0u; i < pointsCount; i++)
        {
            var point = ReadGeometry(reader) as Point;
            if (point != null)
                points.Add(point);
            else
                throw new SerializationException("Geometry not a point.");
        }

        return new MultiPoint(points);
    }

    private MultiLineString ReadMultiLineString(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var pointsCount = reader.ReadUInt32();
        var lineStrings = new List<LineString>();
        for (var i = 0u; i < pointsCount; i++)
        {
            var lineString = ReadGeometry(reader) as LineString;
            if (lineString != null)
                lineStrings.Add(lineString);
            else
                throw new SerializationException("Geometry not a linestring.");
        }

        return new MultiLineString(lineStrings);
    }

    private MultiPolygon ReadMultiPolygon(WkbBinaryReader reader, WkbDimensions dimensions)
    {
        var pointsCount = reader.ReadUInt32();
        var polygons = new List<Polygon>();
        for (var i = 0u; i < pointsCount; i++)
        {
            var polygon = ReadGeometry(reader) as Polygon;
            if (polygon != null)
                polygons.Add(polygon);
            else
                throw new SerializationException("Geometry not a polygon.");
        }

        return new MultiPolygon(polygons);
    }

    private GeometryCollection ReadGeometryCollection(WkbBinaryReader reader)
    {
        var pointsCount = reader.ReadUInt32();
        var geometries = new List<IGeometry>();
        for (var i = 0u; i < pointsCount; i++)
            geometries.Add(ReadGeometry(reader));
        return new GeometryCollection(geometries);
    }
}
