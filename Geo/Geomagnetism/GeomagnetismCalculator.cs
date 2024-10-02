using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Geodesy;

namespace Geo.Geomagnetism;

public class GeomagnetismCalculator
{
    private readonly Spheroid _spheroid;

    public GeomagnetismCalculator()
        : this(Spheroid.Default, null) { }

    public GeomagnetismCalculator(IEnumerable<IGeomagneticModel> geomagneticModels)
        : this(Spheroid.Default, geomagneticModels) { }

    public GeomagnetismCalculator(Spheroid spheroid)
        : this(spheroid, null) { }

    public GeomagnetismCalculator(
        Spheroid spheroid,
        IEnumerable<IGeomagneticModel> geomagneticModels
    )
    {
        _spheroid = spheroid;
        if (geomagneticModels != null)
            Models.AddRange(geomagneticModels);
    }

    public List<IGeomagneticModel> Models { get; } = new();

    public GeomagnetismResult TryCalculate(IPosition position, DateTimeOffset date)
    {
        return TryCalculate(position, date.UtcDateTime);
    }

    public GeomagnetismResult TryCalculate(IPosition position, DateTime utcDate)
    {
        GeomagnetismResult result;
        TryCalculate(position, utcDate, out result);
        return result;
    }

    public bool TryCalculate(IPosition position, DateTimeOffset date, out GeomagnetismResult result)
    {
        return TryCalculate(position, date.UtcDateTime, out result);
    }

    public bool TryCalculate(IPosition position, DateTime utcDate, out GeomagnetismResult result)
    {
        var coordinate = position.GetCoordinate();
        var coordinateZ =
            coordinate as CoordinateZ
            ?? new CoordinateZ(coordinate.Latitude, coordinate.Longitude, 0);

        double lat = coordinateZ.Latitude.ToRadians(),
            lon = coordinateZ.Longitude.ToRadians(),
            ele = coordinateZ.Elevation / 1000,
            dat = JulianDate.JD(utcDate);

        var model = Models.SingleOrDefault(mod =>
            mod.ValidFrom <= utcDate && mod.ValidTo > utcDate
        );

        if (model == null)
        {
            result = default;
            return false;
        }

        var bound = 1 + model.MainCoefficientsG.GetUpperBound(0);

        var sinLat = Math.Sin(lat);
        var cosLat = Math.Cos(lat);
        var a = _spheroid.EquatorialAxis / 1000;
        var f = _spheroid.Flattening;
        var b = a * (1.0 - f);

        var sinLat2 = sinLat * sinLat;
        var cosLat2 = cosLat * cosLat;
        var a2 = a * a;
        var a4 = a2 * a2;
        var b2 = b * b;
        var b4 = b2 * b2;

        var sr = Math.Sqrt(a2 * cosLat2 + b2 * sinLat2);
        var theta = Math.Atan2(cosLat * (ele * sr + a2), sinLat * (ele * sr + b2));
        var r =
            ele * ele + 2.0 * ele * sr + (a4 - (a4 - b4) * sinLat2) / (a2 - (a2 - b2) * sinLat2);

        r = Math.Sqrt(r);

        var c = Math.Cos(theta);
        var s = Math.Sin(theta);

        double invS;
        if (Math.Abs(s - 0) < double.Epsilon)
            invS = 1.0 / (s + 1E-08);
        else
            invS = 1.0 / (s + 0.0);

        var p = new double[bound, bound];
        var dp = new double[bound, bound];

        p[0, 0] = 1;
        p[1, 1] = s;
        dp[0, 0] = 0;
        dp[1, 1] = c;
        p[1, 0] = c;
        dp[1, 0] = -s;

        for (var i = 2; i < bound; i++)
        {
            var root = Math.Sqrt((2.0 * i - 1) / (2.0 * i));
            p[i, i] = p[i - 1, i - 1] * s * root;
            dp[i, i] = (dp[i - 1, i - 1] * s + p[i - 1, i - 1] * c) * root;
        }

        for (var i = 0; i < bound; i++)
        {
            double i2 = i * i;
            for (var j = Math.Max(i + 1, 2); j < bound; j++)
            {
                var root1 = Math.Sqrt((j - 1) * (j - 1) - i2);
                var root2 = 1.0 / Math.Sqrt(j * j - i2);
                p[j, i] = (p[j - 1, i] * c * (2.0 * j - 1) - p[j - 2, i] * root1) * root2;
                dp[j, i] =
                    ((dp[j - 1, i] * c - p[j - 1, i] * s) * (2.0 * j - 1) - dp[j - 2, i] * root1)
                    * root2;
            }
        }

        double[,] g = new double[bound, bound],
            h = new double[bound, bound];
        double bRadial = 0.0,
            bTheta = 0.0,
            bPhi = 0.0;
        var fn0 = _spheroid.MeanRadius / 1000 / r;
        var fn = fn0 * fn0;

        double[] sm = new double[bound],
            cm = new double[bound];
        sm[0] = Math.Sin(0);
        cm[0] = Math.Cos(0);

        var yearfrac = (dat - JulianDate.JD(model.ValidFrom)) / 365.25;
        for (var i = 1; i < bound; i++)
        {
            sm[i] = Math.Sin(i * lon);
            cm[i] = Math.Cos(i * lon);

            for (var j = 0; j < bound; j++)
            {
                g[i, j] =
                    model.MainCoefficientsG[i, j] + yearfrac * model.SecularCoefficientsG[i, j];
                h[i, j] =
                    model.MainCoefficientsH[i, j] + yearfrac * model.SecularCoefficientsH[i, j];
            }

            double c1 = 0,
                c2 = 0,
                c3 = 0;
            for (var j = 0; j <= i; j++)
            {
                var c0 = g[i, j] * cm[j] + h[i, j] * sm[j];
                c1 += c0 * p[i, j];
                c2 += c0 * dp[i, j];
                c3 += j * (g[i, j] * sm[j] - h[i, j] * cm[j]) * p[i, j];
            }

            fn *= fn0;
            bRadial += (i + 1) * c1 * fn;
            bTheta -= c2 * fn;
            bPhi += c3 * fn * invS;
        }

        var psi = theta - (Math.PI / 2.0 - lat);
        var sinPsi = Math.Sin(psi);
        var cosPsi = Math.Cos(psi);

        var x = -bTheta * cosPsi - bRadial * sinPsi;
        var y = bPhi;
        var z = bTheta * sinPsi - bRadial * cosPsi;
        result = new GeomagnetismResult(
            coordinateZ,
            new DateTime(utcDate.Ticks, DateTimeKind.Utc),
            x,
            y,
            z
        );
        return true;
    }
}
