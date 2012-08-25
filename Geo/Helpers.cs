using System;

namespace Geo
{
    internal static class Helpers
    {
        internal static double ToRadians(this double degrees)
        {
            return degrees * Math.PI / 180.0;
        }

        internal static double ToDegrees(this double radians)
        {
            return radians*180/Math.PI;
        }
    }
}
