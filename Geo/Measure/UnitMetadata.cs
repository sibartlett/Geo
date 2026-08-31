#nullable enable
using System;

namespace Geo.Measure;

// Resolves a unit's symbol and SI conversion factor.
//
// The lookups are written out rather than reflected over the enums' [Unit]
// attributes, which is what this did before. Reflection made the whole Measure
// namespace unusable under NativeAOT - Enum.GetValues(Type) requires runtime code
// generation, and the enum fields the factors hung off were only ever reached by
// name, so nothing stopped the trimmer removing them. Written out, the data costs
// no reflection, no static-constructor dictionary building, and is visible to the
// compiler.
public class UnitMetadata
{
    private static readonly UnitDefinition AreaM = new("m²", 1 * 1);
    private static readonly UnitDefinition AreaNm = new("nm²", 1852 * 1852);
    private static readonly UnitDefinition AreaKm = new("km²", 1000 * 1000);
    private static readonly UnitDefinition AreaMile = new("mi²", 1609.344 * 1609.344);
    private static readonly UnitDefinition AreaFt = new("ft²", 0.3048 * 0.3048);

    private static readonly UnitDefinition DistanceM = new("m", 1);
    private static readonly UnitDefinition DistanceNm = new("nm", 1852);
    private static readonly UnitDefinition DistanceKm = new("km", 1000);

    // The international mile is exactly 1609.344 m; the rounded 1609.34 used before
    // left every mile 4 mm short.
    private static readonly UnitDefinition DistanceMile = new("mi", 1609.344);
    private static readonly UnitDefinition DistanceFt = new("ft", 0.3048);

    private static readonly UnitDefinition SpeedMs = new("m/s", 1);

    // The factors are written as the exact ratios that define the units rather than as
    // rounded decimals: 0.277778 m/s is not a kilometre per hour, and converting 10 m/s
    // through it gave 35.99997 kph instead of 36.
    private static readonly UnitDefinition SpeedKnots = new("knots", 1852d / 3600d);
    private static readonly UnitDefinition SpeedKph = new("kph", 1000d / 3600d);
    private static readonly UnitDefinition SpeedMph = new("mph", 0.44704);

    internal static UnitDefinition For(AreaUnit unit)
    {
        switch (unit)
        {
            case AreaUnit.M:
                return AreaM;
            case AreaUnit.Nm:
                return AreaNm;
            case AreaUnit.Km:
                return AreaKm;
            case AreaUnit.Mile:
                return AreaMile;
            case AreaUnit.Ft:
                return AreaFt;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit));
        }
    }

    internal static UnitDefinition For(DistanceUnit unit)
    {
        switch (unit)
        {
            case DistanceUnit.M:
                return DistanceM;
            case DistanceUnit.Nm:
                return DistanceNm;
            case DistanceUnit.Km:
                return DistanceKm;
            case DistanceUnit.Mile:
                return DistanceMile;
            case DistanceUnit.Ft:
                return DistanceFt;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit));
        }
    }

    internal static UnitDefinition For(SpeedUnit unit)
    {
        switch (unit)
        {
            case SpeedUnit.Ms:
                return SpeedMs;
            case SpeedUnit.Knots:
                return SpeedKnots;
            case SpeedUnit.Kph:
                return SpeedKph;
            case SpeedUnit.Mph:
                return SpeedMph;
            default:
                throw new ArgumentOutOfRangeException(nameof(unit));
        }
    }
}
