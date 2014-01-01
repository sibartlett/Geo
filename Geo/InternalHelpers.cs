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

        public static double NormalizeDegrees(this double degrees)
        {
            while (degrees >= 360)
                degrees -= 360;
            while (degrees < 0)
                degrees += 360;
            return degrees;
        }
    }
}
