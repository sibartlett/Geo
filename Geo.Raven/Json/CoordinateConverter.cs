using System;
using Raven.Imports.Newtonsoft.Json;

namespace Geo.Raven.Json
{
    public class CoordinateConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var coordinate = value as Coordinate;
            
            if (coordinate == null)
            {
                writer.WriteNull();
                return;
            }

            writer.WriteStartArray();
            writer.WriteValue(coordinate.Longitude);
            writer.WriteValue(coordinate.Latitude);
            if (coordinate.Is3D || coordinate.IsMeasured)
                writer.WriteValue(coordinate.Elevation);
            if (coordinate.IsMeasured)
                writer.WriteValue(coordinate.M);
            writer.WriteEndArray();
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var arr = serializer.Deserialize<double[]>(reader);

            if (arr != null)
            {
                if (arr.Length == 2)
                    return new Coordinate(arr[1], arr[0]);

                if (arr.Length == 3)
                    return new Coordinate(arr[1], arr[0], arr[2]);

                if (arr.Length == 4)
                    return new Coordinate(arr[1], arr[0], arr[2], arr[3]);
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(Coordinate) == objectType;
        }
    }
}
