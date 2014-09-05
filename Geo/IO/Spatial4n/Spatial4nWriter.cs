using System.Globalization;
using System.Runtime.Serialization;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.IO.Wkt;

namespace Geo.IO.Spatial4n
{
    public class Spatial4nWriter
    {
        private readonly WktWriterSettings _wktWriterSettings;

        public Spatial4nWriter()
        {
            _wktWriterSettings = WktWriterSettings.NtsCompatible;
            _wktWriterSettings.MaxDimesions = 2;
        }

        public string Write(ISpatial4nShape geometry)
        {
            if (ReferenceEquals(null, geometry))
                return null;

            string result;

            if (TryWritePoint(geometry, out result))
                return result;

            if (TryWriteCircle(geometry, out result))
                return result;

            if (TryWriteEnvelope(geometry, out result))
                return result;

            var wkt = geometry as IGeometry;
            if (wkt != null)
                return wkt.ToWktString(_wktWriterSettings);

            throw new SerializationException("Object of type " + geometry.GetType().Name + " is not supported by Spatial4n.");
        }

        protected virtual double ConvertCircleRadius(double radius)
        {
            return (radius/Constants.EarthMeanRadius).ToDegrees();
        }

        private bool TryWritePoint(ISpatial4nShape shape, out string result)
        {
            var point = shape as Point;
            if (point != null)
            {
                if (point.IsEmpty)
                    result = default(string);
                else
                    result = string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", point.Coordinate.Longitude, point.Coordinate.Latitude);
                return true;
            }
            result = default(string);
            return false;
        }

        private bool TryWriteCircle(ISpatial4nShape shape, out string result)
        {
            var circle = shape as Circle;
            if (circle != null)
            {
                if (circle.IsEmpty)
                    result = default(string);
                else
                    result = string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F6} {1:F6} d={2:F6})", circle.Center.Longitude, circle.Center.Latitude, ConvertCircleRadius(circle.Radius));
                return true;
            }
            result = default(string);
            return false;
        }

        private bool TryWriteEnvelope(ISpatial4nShape shape, out string result)
        {
            var envelope = shape as Envelope;
            if (envelope != null)
            {
                result = string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6} {2:F6} {3:F6}", envelope.MinLon,
                                     envelope.MinLat, envelope.MaxLon, envelope.MaxLat);
                return true;
            }
            result = default(string);
            return false;
        }
    }
}
