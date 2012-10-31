using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.Interfaces;

namespace Geo.Json
{
    public class GeoJsonWriter
    {
        public string Write(IGeoJsonObject obj)
        {
            var geometry = obj as IGeoJsonGeometry;
            if (geometry != null)
                return Write(geometry);

            var feature = obj as Feature;
            if (feature != null)
                return Write(feature);

            var featureCollection = obj as FeatureCollection;
            if (featureCollection != null)
                return Write(featureCollection);

            throw new SerializationException("Object of type '" + obj.GetType().Name + "' is not supported by GeoJSON");
        }

        public string Write(IGeoJsonGeometry geometry)
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

            throw new SerializationException("Geometry of type '" + geometry.GetType().Name + "' is not supported by GeoJSON");
        }

        private Dictionary<string, object> WritePoint(Point point)
        {
            return new Dictionary<string, object>
            {
                { "type", "Point" },
                { "coordinates", WriteCoordinate(point) }
            };
        }

        private Dictionary<string, object> WriteLineString(LineString lineString)
        {
            return new Dictionary<string, object>
            {
                { "type", "LineString" },
                { "coordinates", WriteCoordinates(lineString.Coordinates) }
            };
        }

        private Dictionary<string, object> WritePolygon(Polygon polygon)
        {
            return new Dictionary<string, object>
            {
                { "type", "Polygon" },
                { "coordinates", WriteCoordinates(polygon) }
            };
        }

        private Dictionary<string, object> WriteMultiPoint(MultiPoint multiPoint)
        {
            return new Dictionary<string, object>
            {
                { "type", "MultiPoint" },
                { "coordinates", multiPoint.Geometries.Select(WriteCoordinate).ToArray() }
            };
        }

        private Dictionary<string, object> WriteMultiLineString(MultiLineString multiLineString)
        {
            return new Dictionary<string, object>
            {
                { "type", "MultiLineString" },
                { "coordinates", multiLineString.Geometries.Select(x=> WriteCoordinates(x.Coordinates)).ToArray() }
            };
        }

        private Dictionary<string, object> WriteMultiPolygon(MultiPolygon multiPolygon)
        {
            return new Dictionary<string, object>
            {
                { "type", "MultiPolygon" },
                { "coordinates", multiPolygon.Geometries.Select(WriteCoordinates).ToArray() }
            };
        }

        private Dictionary<string, object> WriteGeometryCollection(GeometryCollection geometryCollection)
        {
            return new Dictionary<string, object>
            {
                { "type", "GeometryCollection" },
                { "geometries", geometryCollection.Geometries.Select(WriteGeometry).ToArray() }
            };
        }

        private Dictionary<string, object> WriteFeature(Feature feature)
        {
            var result = new Dictionary<string, object>
            {
                { "type", "Feature" },
                { "geometry", WriteGeometry(feature.Geometry) }
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
                { "features", featureCollection.Features.Select(WriteFeature).ToArray() }
            };
        }


        private double[] WriteCoordinate(IPosition position)
        {
            var point = position.GetCoordinate();
            if (point.HasElevation && point.HasM)
                return new[] { point.Longitude, point.Latitude, point.Elevation, point.M };
            if (point.HasElevation)
                return new[] { point.Longitude, point.Latitude, point.Elevation };
            if (point.HasM)
                return new[] { point.Longitude, point.Latitude, point.M };
            return new[] { point.Longitude, point.Latitude };
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
}
