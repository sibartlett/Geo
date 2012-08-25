using System;
using Geo.Gps;
using Geo.Measure;

namespace Geo.Geometries
{
    public static class LineStringExtensions
    {
        public static Speed GetAverageSpeed<T>(this LineString<T> lineString) where T : Fix
        {
            var distance = lineString.CalculateLength();
            return new Speed(distance.SiValue, lineString.GetDuration());
        }

        public static TimeSpan GetDuration<T>(this LineString<T> lineString) where T : Fix
        {
            return lineString.StartPoint.TimeUtc - lineString.EndPoint.TimeUtc;
        }

        public static DateTime GetStartTime<T>(this LineString<T> lineString) where T : Fix
        {
            return lineString.StartPoint.TimeUtc;
        }

        public static DateTime GetEndTime<T>(this LineString<T> lineString) where T : Fix
        {
            return lineString.EndPoint.TimeUtc;
        }
    }
}
