using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Geo
{
    internal static class InternalHelpers
    {
        public static double ToRadians(this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        public static double ToDegrees(this double radians)
        {
            return radians*180/Math.PI;
        }

        public static IEnumerable<T> GetEnumValues<T>()
        {
            var type = typeof(T);
            return type
                .GetFields(BindingFlags.Static | BindingFlags.Public)
                .Select(fi => (T) Enum.Parse(type, fi.Name, false));
        }

        public static bool IsNullOrWhitespace(this string value)
        {
            return value == null || value.Trim().Length == 0;
        }
    }
}
