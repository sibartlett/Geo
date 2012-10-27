using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Geo.Geometries;
using Geo.Interfaces;

namespace Geo.Json
{
    public static class GeoJson
    {
        public static string Serialize(IGeoJsonObject obj)
        {
            return obj.ToGeoJson();
        }

        public static IGeoJsonObject Deserialize(string json)
        {
            object obj;
            if (TryDeserialize(json, out obj))
                return (IGeoJsonObject) obj;
            throw new SerializationException("Invalid GeoJSON string");
        }

        public static bool TryDeserialize(string json, out object result)
        {
            result = null;
            object obj;

            if (!SimpleJson.TryDeserializeObject(json, out obj))
                return false;

            if (obj is JsonObject)
            {
                if (TryParseGeometry((JsonObject)obj, out result))
                    return true;
                if (TryParseFeature((JsonObject)obj, out result))
                    return true;
                if (TryParseFeatureCollection((JsonObject)obj, out result))
                    return true;
            }

            return false;
        }

        private static bool TryParseTypeString(JsonObject obj, out string result)
        {
            object type = null;
            if (obj != null)
                obj.TryGetValue("type", out type);
                
            result = type as string;
            return type != null;
        }

        private static bool TryParseFeatureCollection(JsonObject obj, out object result)
        {
            result = null;
            string typeString;
            if (TryParseTypeString(obj, out typeString) && typeString.ToLowerInvariant() == "featurecollection")
            {
                var features = obj["features"] as JsonArray;

                if (features != null)
                {
                    var temp = new object[features.Count];
                    for (var index = 0; index < features.Count; index++)
                    {
                        var geometry = features[index];
                        if (!TryParseFeature((JsonObject) geometry, out temp[index]))
                            return false;
                    }
                    result = new FeatureCollection(temp.Cast<Feature>());
                    return true;
                }
            }
            return false;
        }

        private static bool TryParseFeature(JsonObject obj, out object result)
        {
            string typeString;
            if(TryParseTypeString(obj, out typeString) && typeString.ToLowerInvariant()== "feature")
            {
                object geometry;
                object geo;
                if (obj.TryGetValue("geometry", out geometry) && TryParseGeometry((JsonObject)geometry, out geo))
                {
                    object prop;
                    Dictionary<string, object> pr = null;
                    if (obj.TryGetValue("properties", out prop) && prop is JsonObject)
                    {
                        var props = (JsonObject) prop;
                        if (props.Count > 0)
                        {
                            pr = props.ToDictionary(x => x.Key, x=> SantizeJsonObjects(x.Value));
                        }
                    }

                    result = new Feature((IGeoJsonGeometry) geo, pr);

                    object id;
                    if (obj.TryGetValue("id", out id))
                    {
                        ((Feature) result).Id = SantizeJsonObjects(id);
                    }

                    return true;
                }
            }
            result = null;
            return false;
        }

        private static bool TryParseGeometry(JsonObject obj, out object result)
        {
            result = null;
            string typeString;
            if (!TryParseTypeString(obj, out typeString))
                return false;

            typeString = typeString.ToLowerInvariant();

            switch (typeString)
            {
                case "point":
                    return TryParsePoint(obj, out result);
                case "linestring":
                    return TryParseLineString(obj, out result);
                case "polygon":
                    return TryParsePolygon(obj, out result);
                case "multipoint":
                    return TryParseMultiPoint(obj, out result);
                case "multilinestring":
                    return TryParseMultiLineString(obj, out result);
                case "multipolygon":
                    return TryParseMultiPolygon(obj, out result);
                case "geometrycollection":
                    return TryParseGeometryCollection(obj, out result);
                default:
                    return false;
            }
        }

        private static bool TryParsePoint(JsonObject obj, out object result)
        {
            result = null;
            var coordinates = obj["coordinates"] as JsonArray;
            if (coordinates == null || coordinates.Count < 2)
                return false;

            Coordinate value;
            if (TryParseCoordinate(coordinates, out value))
            {
                result = value.ToPoint();
                return true;
            }
            return false;
        }

        private static bool TryParseLineString(JsonObject obj, out object result)
        {
            var coordinates = obj["coordinates"] as JsonArray;
            Coordinate[] co;
            if (coordinates != null && TryParseCoordinateArray(coordinates, out co))
            {
                result = new LineString(co);
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryParsePolygon(JsonObject obj, out object result)
        {
            var coordinates = obj["coordinates"] as JsonArray;

            Coordinate[][] temp;
            if (coordinates != null && coordinates.Count > 0 && TryParseCoordinateArrayArray(coordinates, out temp))
            {
                result = new Polygon(new LinearRing(temp[0]), temp.Skip(1).Select(x => new LinearRing(x)));
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryParseMultiPoint(JsonObject obj, out object result)
        {
            var coordinates = obj["coordinates"] as JsonArray;
            Coordinate[] co;
            if (coordinates != null && TryParseCoordinateArray(coordinates, out co))
            {
                result = new MultiPoint(co.Select(x => x.ToPoint()));
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryParseMultiLineString(JsonObject obj, out object result)
        {
            var coordinates = obj["coordinates"] as JsonArray;
            Coordinate[][] co;
            if (coordinates != null && TryParseCoordinateArrayArray(coordinates, out co))
            {
                result = new MultiLineString(co.Select(x => new LineString(x)));
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryParseMultiPolygon(JsonObject obj, out object result)
        {
            var coordinates = obj["coordinates"] as JsonArray;
            Coordinate[][][] co;
            if (coordinates != null && TryParseCoordinateArrayArrayArray(coordinates, out co))
            {
                result = new MultiPolygon(co.Select(x =>
                    new Polygon(new LinearRing(x.First()), x.Skip(1).Select(c => new LinearRing(c)))
                    ));
                return true;
            }
            result = null;
            return false;
        }

        private static bool TryParseGeometryCollection(JsonObject obj, out object result)
        {
            result = null;
            var geometries = obj["geometries"] as JsonArray;

            if (geometries != null)
            {
                var temp =new object[geometries.Count];
                for (var index = 0; index < geometries.Count; index++)
                {
                    var geometry = geometries[index];
                    if (!TryParseGeometry((JsonObject)geometry, out temp[index]))
                        return false;
                }
                result = new GeometryCollection(temp.Cast<IGeometry>());
                return true;
            }
            return false;
        }

        private static bool TryParseCoordinate(JsonArray coordinates, out Coordinate result)
        {
            result = null;
            if (coordinates == null || coordinates.Count < 2)
                return false;

            var valid = coordinates.All(x => x is double || x is long);
            if (!valid)
                return false;

            if (coordinates.Count == 2)
                result = new Point(Convert.ToDouble(coordinates[1]), Convert.ToDouble(coordinates[0]));
            else
                result = new Point(Convert.ToDouble(coordinates[1]), Convert.ToDouble(coordinates[0]), Convert.ToDouble(coordinates[2]));
            return true;
        }

        private static bool TryParseCoordinateArray(JsonArray coordinates, out Coordinate[] result)
        {
            result = null;
            if (coordinates == null)
                return false;

            var valid = coordinates.All(x => x is JsonArray);
            if (!valid)
                return false;

            var tempResult = new Coordinate[coordinates.Count];
            for (var index = 0; index < coordinates.Count; index++)
            {
                if (!TryParseCoordinate((JsonArray)coordinates[index], out tempResult[index]))
                    return false;
            }
            result = tempResult;
            return true;
        }

        private static bool TryParseCoordinateArrayArray(JsonArray coordinates, out Coordinate[][] result)
        {
            result = null;
            if (coordinates == null)
                return false;

            var valid = coordinates.All(x => x is JsonArray);
            if (!valid)
                return false;

            var tempResult = new Coordinate[coordinates.Count][];
            for (var index = 0; index < coordinates.Count; index++)
            {
                if (!TryParseCoordinateArray((JsonArray)coordinates[index], out tempResult[index]))
                    return false;
            }
            result = tempResult;
            return true;
        }

        private static bool TryParseCoordinateArrayArrayArray(JsonArray coordinates, out Coordinate[][][] result)
        {
            result = null;
            if (coordinates == null)
                return false;

            var valid = coordinates.All(x => x is JsonArray);
            if (!valid)
                return false;

            var tempResult = new Coordinate[coordinates.Count][][];
            for (var index = 0; index < coordinates.Count; index++)
            {
                if (!TryParseCoordinateArrayArray((JsonArray)coordinates[index], out tempResult[index]))
                    return false;
            }
            result = tempResult;
            return true;
        }

        public static object SantizeJsonObjects(object obj)
        {
            if (obj is JsonArray)
                return ((JsonArray) obj).Select(SantizeJsonObjects).ToArray();
            else if (obj is JsonObject)
                return ((JsonObject) obj).ToDictionary(x => x.Key, x => SantizeJsonObjects(x));
            else
                return obj;
        }
    }
}
