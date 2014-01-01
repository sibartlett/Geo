using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;

namespace Geo.Measure
{
    public class UnitMetadata
    {
        private static readonly Dictionary<AreaUnit, UnitAttribute> AreaCache;
        private static readonly Dictionary<DistanceUnit, UnitAttribute> DistanceCache;
        private static readonly Dictionary<SpeedUnit, UnitAttribute> SpeedCache;

        static UnitMetadata()
        {
            AreaCache = Init<AreaUnit>();
            DistanceCache = Init<DistanceUnit>();
            SpeedCache = Init<SpeedUnit>();
        }

        internal static UnitAttribute For(AreaUnit unit)
        {
            return AreaCache[unit];
        }

        internal static UnitAttribute For(DistanceUnit unit)
        {
            return DistanceCache[unit];
        }

        internal static UnitAttribute For(SpeedUnit unit)
        {
            return SpeedCache[unit];
        }

        private static Dictionary<T, UnitAttribute> Init<T>()
        {
            var type = typeof(T);
            var a = new Dictionary<T, UnitAttribute>();
            foreach (T unit in Enum.GetValues(typeof(T)))
            {
                var name = Enum.GetName(type, unit);

                var attr = type.GetField(name)
                    .GetCustomAttributes(typeof(UnitAttribute), false)
                    .Cast<UnitAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                    a.Add(unit, attr);
            }

            return new Dictionary<T, UnitAttribute>(a);
        }
    }
}
