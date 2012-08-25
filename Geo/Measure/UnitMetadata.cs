using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Geo.Measure
{
    public class UnitMetadata
    {
        private static readonly ReadOnlyDictionary<DistanceUnit, UnitAttribute> DistanceCache;
        private static readonly ReadOnlyDictionary<SpeedUnit, UnitAttribute> SpeedCache;

        static UnitMetadata()
        {
            DistanceCache = Init<DistanceUnit>();
            SpeedCache = Init<SpeedUnit>();
        }

        internal static UnitAttribute For(DistanceUnit unit)
        {
            return DistanceCache[unit];
        }

        internal static UnitAttribute For(SpeedUnit unit)
        {
            return SpeedCache[unit];
        }

        private static ReadOnlyDictionary<T, UnitAttribute> Init<T>()
        {
            var type = typeof (T);
            var a = new Dictionary<T, UnitAttribute>();
            foreach (T unit in Enum.GetValues(type))
            {
                var name = Enum.GetName(type, unit);
                var attr = type.GetField(name)
                    .GetCustomAttributes(typeof(UnitAttribute), false)
                    .Cast<UnitAttribute>()
                    .FirstOrDefault();

                if (attr != null)
                    a.Add(unit, attr);
            }

            return new ReadOnlyDictionary<T, UnitAttribute>(a);
        }
    }
}
