using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;

namespace Geo.IO.GeoJson;

public class GeoJsonWriter
{
    private readonly GeoJsonWriterSettings _settings;

    public GeoJsonWriter()
    {
        _settings = new GeoJsonWriterSettings();
    }

    public GeoJsonWriter(GeoJsonWriterSettings settings)
    {
        _settings = settings;
    }

    public string Write(object obj)
    {
        var geometry = obj as IGeometry;
        if (geometry != null)
            return Write(geometry);

        var feature = obj as Feature;
        if (feature != null)
            return Write(feature);

        var featureCollection = obj as FeatureCollection;
        if (featureCollection != null)
            return Write(featureCollection);

        throw new SerializationException(
            "Object of type '" + obj.GetType().Name + "' is not supported by GeoJSON"
        );
    }

    public string Write(IGeometry geometry)
    {
        return SimpleJson.SerializeObject(WriteGeometry(geometry));
    }

    public string Write(Feature feature)
    {
        return SimpleJson.SerializeObject(WriteFeature(feature));
    }

    public string Write(FeatureCollection featureCollection)
    {
        return SimpleJson.SerializeObject(WriteFeatureCollection(featureCollection));
    }

    private Dictionary<string, object> WriteGeometry(IGeometry geometry)
    {
        var point = geometry as Point;
        if (point != null)
            return WritePoint(point);

        var lineString = geometry as LineString;
        if (lineString != null)
            return WriteLineString(lineString);

        var polygon = geometry as Polygon;
        if (polygon != null)
            return WritePolygon(polygon);

        var multiPoint = geometry as MultiPoint;
        if (multiPoint != null)
            return WriteMultiPoint(multiPoint);

        var multiLineString = geometry as MultiLineString;
        if (multiLineString != null)
            return WriteMultiLineString(multiLineString);

        var multiPolygon = geometry as MultiPolygon;
        if (multiPolygon != null)
            return WriteMultiPolygon(multiPolygon);

        var collection = geometry as GeometryCollection;
        if (collection != null)
            return WriteGeometryCollection(collection);

        var circle = geometry as Circle;
        if (_settings.ConvertCirclesToRegularPolygons)
            if (circle != null)
                return WritePolygon(circle.ToPolygon(_settings.CircleSides));

        throw new SerializationException(
            "Geometry of type '" + geometry.GetType().Name + "' is not supported by GeoJSON"
        );
    }

    private Dictionary<string, object> WritePoint(Point point)
    {
        return new Dictionary<string, object>
        {
            { "type", "Point" },
            { "coordinates", WriteCoordinate(point) },
        };
    }

    private Dictionary<string, object> WriteLineString(LineString lineString)
    {
        return new Dictionary<string, object>
        {
            { "type", "LineString" },
            { "coordinates", WriteCoordinates(lineString.Coordinates) },
        };
    }

    private Dictionary<string, object> WritePolygon(Polygon polygon)
    {
        return new Dictionary<string, object>
        {
            { "type", "Polygon" },
            { "coordinates", WriteCoordinates(polygon) },
        };
    }

    private Dictionary<string, object> WriteMultiPoint(MultiPoint multiPoint)
    {
        return new Dictionary<string, object>
        {
            { "type", "MultiPoint" },
            {
                "coordinates",
                multiPoint.Geometries.Cast<Point>().Select(WriteCoordinate).ToArray()
            },
        };
    }

    private Dictionary<string, object> WriteMultiLineString(MultiLineString multiLineString)
    {
        return new Dictionary<string, object>
        {
            { "type", "MultiLineString" },
            {
                "coordinates",
                multiLineString
                    .Geometries.Cast<LineString>()
                    .Select(x => WriteCoordinates(x.Coordinates))
                    .ToArray()
            },
        };
    }

    private Dictionary<string, object> WriteMultiPolygon(MultiPolygon multiPolygon)
    {
        return new Dictionary<string, object>
        {
            { "type", "MultiPolygon" },
            {
                "coordinates",
                multiPolygon.Geometries.Cast<Polygon>().Select(WriteCoordinates).ToArray()
            },
        };
    }

    private Dictionary<string, object> WriteGeometryCollection(
        GeometryCollection geometryCollection
    )
    {
        return new Dictionary<string, object>
        {
            { "type", "GeometryCollection" },
            { "geometries", geometryCollection.Geometries.Select(WriteGeometry).ToArray() },
        };
    }

    private Dictionary<string, object> WriteFeature(Feature feature)
    {
        var result = new Dictionary<string, object>
        {
            { "type", "Feature" },
            { "geometry", feature.Geometry == null ? null : WriteGeometry(feature.Geometry) },
        };

        if (feature.Properties != null && feature.Properties.Count > 0)
            result.Add("properties", feature.Properties);
        else
            result.Add("properties", null);

        if (feature.Id != null)
            result.Add("id", feature.Id);

        return result;
    }

    private Dictionary<string, object> WriteFeatureCollection(FeatureCollection featureCollection)
    {
        return new Dictionary<string, object>
        {
            { "type", "FeatureCollection" },
            { "features", featureCollection.Features.Select(WriteFeature).ToArray() },
        };
    }

    private double[] WriteCoordinate(IPosition position)
    {
        var coordinate = position.Coordinate;
        var pointZM = coordinate as CoordinateZM;
        if (pointZM != null)
            return new[]
            {
                pointZM.Longitude,
                pointZM.Latitude,
                pointZM.Elevation,
                pointZM.Measure,
            };
        var pointZ = coordinate as CoordinateZ;
        if (pointZ != null)
            return new[] { pointZ.Longitude, pointZ.Latitude, pointZ.Elevation };
        //CoordinateM is not supported by GeoJSON
        return new[] { coordinate.Longitude, coordinate.Latitude };
    }

    private IEnumerable<double[]> WriteCoordinates(CoordinateSequence sequence)
    {
        return sequence.Select(WriteCoordinate).ToArray();
    }

    private IEnumerable<IEnumerable<double[]>> WriteCoordinates(Polygon polygon)
    {
        yield return WriteCoordinates(polygon.Shell.Coordinates);
        foreach (var hole in polygon.Holes)
            yield return WriteCoordinates(hole.Coordinates);
    }
}
