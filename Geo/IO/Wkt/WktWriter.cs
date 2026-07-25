#nullable enable
using System.Globalization;
using System.Runtime.Serialization;
using System.Text;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO.Wkt;

public class WktWriter
{
    private readonly WktWriterSettings _settings;

    public WktWriter()
    {
        _settings = new WktWriterSettings();
    }

    public WktWriter(WktWriterSettings settings)
    {
        _settings = settings;
    }

    public string Write(IGeometry geometry)
    {
        var builder = new StringBuilder();
        AppendGeometry(builder, geometry);
        return builder.ToString();
    }

    private void AppendGeometry(StringBuilder builder, IGeometry geometry)
    {
        var point = geometry as Point;
        if (point != null)
        {
            AppendPoint(builder, point);
            return;
        }

        if (_settings.LinearRing)
        {
            var linearRing = geometry as LinearRing;
            if (linearRing != null)
            {
                AppendLinearRing(builder, linearRing);
                return;
            }
        }

        var lineString = geometry as LineString;
        if (lineString != null)
        {
            AppendLineString(builder, lineString);
            return;
        }

        if (_settings.Triangle)
        {
            var triangle = geometry as Triangle;
            if (triangle != null)
            {
                AppendTriangle(builder, triangle);
                return;
            }
        }

        var polygon = geometry as Polygon;
        if (polygon != null)
        {
            AppendPolygon(builder, polygon);
            return;
        }

        var multiPoint = geometry as MultiPoint;
        if (multiPoint != null)
        {
            AppendMultiPoint(builder, multiPoint);
            return;
        }

        var multiLineString = geometry as MultiLineString;
        if (multiLineString != null)
        {
            AppendMultiLineString(builder, multiLineString);
            return;
        }

        var multiPolygon = geometry as MultiPolygon;
        if (multiPolygon != null)
        {
            AppendMultiPolygon(builder, multiPolygon);
            return;
        }

        var geometryCollection = geometry as GeometryCollection;
        if (geometryCollection != null)
        {
            AppendGeometryCollection(builder, geometryCollection);
            return;
        }

        if (_settings.ConvertCirclesToRegularPolygons)
        {
            var circle = geometry as Circle;
            if (circle != null)
            {
                AppendPolygon(builder, circle.ToPolygon(_settings.CircleSides));
                return;
            }
        }

        throw new SerializationException(
            "Geometry of type '" + geometry.GetType().Name + "' is not supported"
        );
    }

    private void AppendPoint(StringBuilder builder, Point point)
    {
        var dimensions = AppendTypeAndDimensions(builder, "POINT", point);
        AppendPointInner(builder, point, dimensions);
    }

    private void AppendPointInner(StringBuilder builder, Point point, GeometryDimensions dimensions)
    {
        if (point.IsEmpty)
        {
            builder.Append("EMPTY");
            return;
        }

        builder.Append("(");
        AppendCoordinate(builder, point.Coordinate!, dimensions);
        builder.Append(")");
    }

    private void AppendLineString(StringBuilder builder, LineString lineString)
    {
        var dimensions = AppendTypeAndDimensions(builder, "LINESTRING", lineString);
        AppendLineStringInner(builder, lineString.Coordinates, dimensions);
    }

    private void AppendLinearRing(StringBuilder builder, LinearRing linearRing)
    {
        var dimensions = AppendTypeAndDimensions(builder, "LINEARRING", linearRing);
        AppendLineStringInner(builder, linearRing.Coordinates, dimensions);
    }

    private void AppendLineStringInner(
        StringBuilder builder,
        CoordinateSequence lineString,
        GeometryDimensions dimensions
    )
    {
        if (lineString.IsEmpty)
        {
            builder.Append("EMPTY");
            return;
        }

        builder.Append("(");
        AppendCoordinates(builder, lineString, dimensions);
        builder.Append(")");
    }

    private void AppendPolygon(StringBuilder builder, Polygon polygon)
    {
        var dimensions = AppendTypeAndDimensions(builder, "POLYGON", polygon);
        AppendPolygonInner(builder, polygon, dimensions);
    }

    private void AppendTriangle(StringBuilder builder, Triangle polygon)
    {
        var dimensions = AppendTypeAndDimensions(builder, "TRIANGLE", polygon);
        AppendPolygonInner(builder, polygon, dimensions);
    }

    private void AppendPolygonInner(
        StringBuilder builder,
        Polygon polygon,
        GeometryDimensions dimensions
    )
    {
        if (polygon.IsEmpty)
        {
            builder.Append("EMPTY");
            return;
        }

        builder.Append("(");
        AppendLineStringInner(builder, polygon.Shell!.Coordinates, dimensions);
        for (var i = 0; i < polygon.Holes.Count; i++)
        {
            builder.Append(", ");
            AppendLineStringInner(builder, polygon.Holes[i].Coordinates, dimensions);
        }

        builder.Append(")");
    }

    private void AppendMultiPoint(StringBuilder builder, MultiPoint multiPoint)
    {
        if (multiPoint.IsEmpty)
        {
            builder.Append("MULTIPOINT EMPTY");
            return;
        }

        var dimensions = AppendTypeAndDimensions(builder, "MULTIPOINT", multiPoint);
        builder.Append("(");
        for (var i = 0; i < multiPoint.Geometries.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");
            AppendPointInner(builder, (Point)multiPoint.Geometries[i], dimensions);
        }

        builder.Append(")");
    }

    private void AppendMultiLineString(StringBuilder builder, MultiLineString multiLineString)
    {
        if (multiLineString.IsEmpty)
        {
            builder.Append("MULTILINESTRING EMPTY");
            return;
        }

        var dimensions = AppendTypeAndDimensions(builder, "MULTILINESTRING", multiLineString);
        builder.Append("(");
        for (var i = 0; i < multiLineString.Geometries.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");
            AppendLineStringInner(
                builder,
                ((LineString)multiLineString.Geometries[i]).Coordinates,
                dimensions
            );
        }

        builder.Append(")");
    }

    private void AppendMultiPolygon(StringBuilder builder, MultiPolygon multiPolygon)
    {
        if (multiPolygon.IsEmpty)
        {
            builder.Append("MULTIPOLYGON EMPTY");
            return;
        }

        var dimensions = AppendTypeAndDimensions(builder, "MULTIPOLYGON", multiPolygon);
        builder.Append("(");
        for (var i = 0; i < multiPolygon.Geometries.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");
            AppendPolygonInner(builder, (Polygon)multiPolygon.Geometries[i], dimensions);
        }

        builder.Append(")");
    }

    private void AppendGeometryCollection(
        StringBuilder builder,
        GeometryCollection geometryCollection
    )
    {
        if (geometryCollection.IsEmpty)
        {
            builder.Append("GEOMETRYCOLLECTION EMPTY");
            return;
        }

        AppendTypeAndDimensions(builder, "GEOMETRYCOLLECTION", geometryCollection);
        builder.Append("(");
        for (var i = 0; i < geometryCollection.Geometries.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");
            // Each member carries its own type and dimension tag, so it declares its
            // own ordinates rather than inheriting the collection's.
            AppendGeometry(builder, geometryCollection.Geometries[i]);
        }

        builder.Append(")");
    }

    // Writes the geometry type and its dimension tag, and returns the dimensions that
    // tag declared so every coordinate written afterwards carries the same ordinates.
    // A single tag covers the whole geometry - including a polygon's holes and the
    // members of a multi-geometry - so the dimensions have to be taken across all of
    // their coordinates, not just the first sequence reached.
    private GeometryDimensions AppendTypeAndDimensions(
        StringBuilder builder,
        string type,
        IGeometry geometry
    )
    {
        var dimensions = GeometryDimensions.For(geometry).Limit(_settings.MaxDimesions);

        builder.Append(type);

        if (_settings.DimensionFlag)
        {
            if (dimensions.Has3D || dimensions.HasMeasure)
                builder.Append(" ");

            if (dimensions.Has3D)
                builder.Append("Z");

            if (dimensions.HasMeasure)
                builder.Append("M");
        }

        builder.Append(" ");
        return dimensions;
    }

    private void AppendCoordinates(
        StringBuilder builder,
        CoordinateSequence coordinates,
        GeometryDimensions dimensions
    )
    {
        for (var i = 0; i < coordinates.Count; i++)
        {
            if (i > 0)
                builder.Append(", ");
            AppendCoordinate(builder, coordinates[i], dimensions);
        }
    }

    // Every coordinate writes the ordinates the dimension tag declared, whether or not
    // it carries them, so that a sequence mixing 2D and 3D coordinates does not produce
    // a geometry whose points disagree with its own tag. A coordinate missing an
    // ordinate gets NullOrdinate ("NaN" by default), which is what the reader takes
    // back as an absent ordinate.
    private void AppendCoordinate(
        StringBuilder builder,
        Coordinate coordinate,
        GeometryDimensions dimensions
    )
    {
        builder.Append(coordinate.Longitude.ToString(CultureInfo.InvariantCulture));
        builder.Append(" ");
        builder.Append(coordinate.Latitude.ToString(CultureInfo.InvariantCulture));

        // With no dimension tag to say whether a third ordinate is an elevation or a
        // measure, a measured geometry has to fill the elevation slot as well to keep
        // the measure in fourth position.
        var appendElevation =
            dimensions.Has3D || (!_settings.DimensionFlag && dimensions.HasMeasure);

        if (appendElevation)
        {
            builder.Append(" ");
            builder.Append(
                coordinate is Is3D elevation
                    ? elevation.Elevation.ToString(CultureInfo.InvariantCulture)
                    : _settings.NullOrdinate
            );
        }

        if (dimensions.HasMeasure)
        {
            builder.Append(" ");
            builder.Append(
                coordinate is IsMeasured measure
                    ? measure.Measure.ToString(CultureInfo.InvariantCulture)
                    : _settings.NullOrdinate
            );
        }
    }
}
