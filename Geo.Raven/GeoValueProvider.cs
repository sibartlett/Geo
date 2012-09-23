using System;
using System.Globalization;
using Geo.Geometries;
using Geo.Gps;
using Geo.Interfaces;
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
            return GetValue(target as IRavenIndexable);
        }

        public string GetValue(IRavenIndexable target)
        {
            var point = target as IPoint;
            if (point != null)
                return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6}", point.Longitude, point.Latitude);

            var envelope = target as Envelope;
            if (envelope != null)
                return string.Format(CultureInfo.InvariantCulture, "{0:F6} {1:F6} {2:F6} {3:F6}", envelope.MinLon, envelope.MinLat, envelope.MaxLon, envelope.MaxLat);

            var circle = target as Circle;
            if (circle != null)
                return string.Format(CultureInfo.InvariantCulture, "CIRCLE({0:F6} {1:F6} d={2:F6})", circle.Center.Longitude, circle.Center.Latitude, circle.Radius / 1000);

            var route = target as Route;
            if (route != null)
                return route.ToLineString().ToWktString();

            var track = target as Track;
            if (track != null)
                return track.ToLineString().ToWktString();

            var wkt = target as IWktShape;
            return wkt == null ? null : wkt.ToWktString();
        }
    }
}