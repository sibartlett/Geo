#nullable enable
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO.Wkb;

public class WkbWriter
{
    private readonly WkbWriterSettings _settings;

    public WkbWriter()
    {
        _settings = new WkbWriterSettings();
    }

    public WkbWriter(WkbWriterSettings settings)
    {
        _settings = settings;
    }

    public byte[] Write(IGeometry geometry)
    {
        using (var stream = new MemoryStream())
        {
            WriteInternal(geometry, stream);
            return stream.ToArray();
        }
    }

    public void Write(IGeometry geometry, Stream stream)
    {
        var bytes = Write(geometry);
        stream.Write(bytes, 0, bytes.Length);
    }

    // Encoding is CPU-bound and buffered in memory; only writing the encoded
    // bytes to the destination stream is genuine I/O, so that is the part made
    // asynchronous.
    public async Task WriteAsync(
        IGeometry geometry,
        Stream stream,
        CancellationToken cancellationToken = default
    )
    {
        var bytes = Write(geometry);
        await stream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
    }

    private void WriteInternal(IGeometry geometry, Stream stream)
    {
        using (var writer = new WkbBinaryWriter(stream, _settings.Encoding))
        {
            Write(geometry, writer);
        }
    }

    private void WriteEncoding(WkbBinaryWriter writer, WkbEncoding encoding)
    {
        writer.Write((byte)encoding);
    }

    private void Write(IGeometry geometry, WkbBinaryWriter writer)
    {
        var point = geometry as Point;
        if (point != null)
        {
            WritePoint(point, writer);
            return;
        }

        var lineString = geometry as LineString;
        if (lineString != null)
        {
            WriteLineString(lineString, writer);
            return;
        }

        if (_settings.Triangle)
        {
            var triangle = geometry as Triangle;
            if (triangle != null)
            {
                WriteTriangle(triangle, writer);
                return;
            }
        }

        var polygon = geometry as Polygon;
        if (polygon != null)
        {
            WritePolygon(polygon, writer);
            return;
        }

        var multiPoint = geometry as MultiPoint;
        if (multiPoint != null)
        {
            WriteMultiPoint(multiPoint, writer);
            return;
        }

        var multiLineString = geometry as MultiLineString;
        if (multiLineString != null)
        {
            WriteMultiLineString(multiLineString, writer);
            return;
        }

        var multiPolygon = geometry as MultiPolygon;
        if (multiPolygon != null)
        {
            WriteMultiPolygon(multiPolygon, writer);
            return;
        }

        var geometryCollection = geometry as GeometryCollection;
        if (geometryCollection != null)
        {
            WriteGeometryCollection(geometryCollection, writer);
            return;
        }

        if (_settings.ConvertCirclesToRegularPolygons)
        {
            var circle = geometry as Circle;
            if (circle != null)
            {
                WritePolygon(circle.ToPolygon(_settings.CircleSides), writer);
                return;
            }
        }

        throw new SerializationException(
            "Geometry of type '" + geometry.GetType().Name + "' is not supported"
        );
    }

    private void WriteCoordinate(Coordinate coordinate, WkbBinaryWriter writer)
    {
        writer.Write(coordinate.Longitude);
        writer.Write(coordinate.Latitude);

        if (coordinate.Is3D && _settings.MaxDimesions > 2)
            writer.Write(((Is3D)coordinate).Elevation);

        if (coordinate.IsMeasured && _settings.MaxDimesions > 3)
            writer.Write(((IsMeasured)coordinate).Measure);
    }

    private void WriteCoordinates(CoordinateSequence coordinates, WkbBinaryWriter writer)
    {
        writer.Write((uint)coordinates.Count);

        foreach (var coordinate in coordinates)
            WriteCoordinate(coordinate, writer);
    }

    private void WritePoint(Point point, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(point, WkbGeometryType.Point, writer);

        if (point.IsEmpty)
        {
            // WKB has no empty flag for a point, so an empty point is encoded as a 2D
            // point with NaN ordinates, matching NTS/GEOS/PostGIS. WriteGeometryType has
            // already written the 2D Point type code for the empty geometry.
            writer.Write(double.NaN);
            writer.Write(double.NaN);
        }
        else
        {
            WriteCoordinate(point.Coordinate!, writer);
        }
    }

    private void WriteLineString(LineString lineString, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(lineString, WkbGeometryType.LineString, writer);
        WriteCoordinates(lineString.Coordinates, writer);
    }

    private void WritePolygon(Polygon polygon, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(polygon, WkbGeometryType.Polygon, writer);
        WritePolygonInner(polygon, writer);
    }

    private void WriteTriangle(Triangle triangle, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(triangle, WkbGeometryType.Triangle, writer);
        WritePolygonInner(triangle, writer);
    }

    private void WritePolygonInner(Polygon polygon, WkbBinaryWriter writer)
    {
        if (polygon.IsEmpty)
        {
            writer.Write(0u);
        }
        else
        {
            writer.Write((uint)(1 + polygon.Holes.Count));
            WriteCoordinates(polygon.Shell!.Coordinates, writer);

            foreach (var hole in polygon.Holes)
                WriteCoordinates(hole.Coordinates, writer);
        }
    }

    private void WriteMultiPoint(MultiPoint multipoint, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(multipoint, WkbGeometryType.MultiPoint, writer);
        writer.Write((uint)multipoint.Geometries.Count);
        foreach (var point in multipoint.Geometries.Cast<Point>())
            Write(point, writer);
    }

    private void WriteMultiLineString(MultiLineString multiLineString, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(multiLineString, WkbGeometryType.MultiLineString, writer);
        writer.Write((uint)multiLineString.Geometries.Count);
        foreach (var linestring in multiLineString.Geometries.Cast<LineString>())
            Write(linestring, writer);
    }

    private void WriteMultiPolygon(MultiPolygon multiPolygon, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(multiPolygon, WkbGeometryType.MultiPolygon, writer);
        writer.Write((uint)multiPolygon.Geometries.Count);
        foreach (var polygon in multiPolygon.Geometries.Cast<Polygon>())
            Write(polygon, writer);
    }

    private void WriteGeometryCollection(GeometryCollection collection, WkbBinaryWriter writer)
    {
        WriteEncoding(writer, _settings.Encoding);
        WriteGeometryType(collection, WkbGeometryType.GeometryCollection, writer);
        writer.Write((uint)collection.Geometries.Count);
        foreach (var geometry in collection.Geometries)
            Write(geometry, writer);
    }

    private void WriteGeometryType(
        IGeometry geometry,
        WkbGeometryType baseType,
        WkbBinaryWriter writer
    )
    {
        if (geometry.IsEmpty)
        {
            writer.Write((uint)baseType);
        }
        else
        {
            var typeCode = (uint)baseType;

            if (geometry.Is3D && _settings.MaxDimesions > 2)
                typeCode += 1000;

            if (geometry.IsMeasured && _settings.MaxDimesions > 3)
                typeCode += 2000;

            writer.Write(typeCode);
        }
    }
}
