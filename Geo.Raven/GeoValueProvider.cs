using System;
using System.Globalization;
using Geo.Geometries;
using Raven.Imports.Newtonsoft.Json.Serialization;

namespace Geo.Raven
{
    public class GeoValueProvider : IValueProvider
    {
        public void SetValue(object target, object value)
        {
            throw new NotSupportedException();
        }

        public object GetValue(object target)
        {
            return GetValue(target as IGeometry);
        }

        public string GetValue(IGeometry target)
        {
            var point = target as ILatLngCoordinate;
            if (point != null)
                return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", point.Longitude, point.Latitude);

            var circle = target as Circle;
            if (circle != null)
                return string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F6} {1:F6} d={2:F6})", circle.Center.Longitude, circle.Center.Latitude, circle.Radius / 1000);

            var wkt = target as IWktShape;
            if (wkt != null)
                return wkt.ToWktString();

            return null;
        }
    }
}