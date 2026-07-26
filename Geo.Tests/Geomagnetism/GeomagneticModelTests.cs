using System;
using System.Collections.Generic;
using System.Linq;
using Geo.Geomagnetism;
using Geo.Geomagnetism.Models;
using Xunit;

namespace Geo.Tests.Geomagnetism;

/// <summary>
/// Checks the geomagnetic coefficient tables themselves, rather than the field they produce.
/// </summary>
/// <remarks>
/// A transcription error in one of these tables is silent: the field it yields stays entirely
/// plausible, and every pinned regression value in
/// <see cref="GeomagnetismCalculatorTests" /> was generated from the tables, so it enshrines
/// whatever they say rather than checking it. WMM and IGRF are separately transcribed here -
/// hand-written per-epoch tables against a single flat array - and they model the same field
/// from the same observations, so setting one against the other is an independent check that
/// needs nothing fetched from outside.
/// <para>
/// This is what found WMM2020's h[8,7] carrying +8.0 where the model has -6.9.
/// </para>
/// <para>
/// One check deliberately absent: comparing each coefficient against the previous epoch's
/// value plus five years of its secular variation. That variation is a forecast, and for
/// small high-degree terms it misses by about as much as the term itself - WMM1985 predicts
/// 32.3 for g[6,5] where 1990 holds 15.4, which IGRF corroborates at 18.0 - so any allowance
/// loose enough to admit that is looser than the h[8,7] error above, and the check cannot
/// tell the two apart. For IGRF it would be circular besides, since
/// <see cref="IgrfModelFactory" /> derives each rate by differencing consecutive epochs.
/// </para>
/// </remarks>
public class GeomagneticModelTests
{
    private static IGeomagneticModel[] Wmm() =>
        new IGeomagneticModel[]
        {
            new Wmm1985(),
            new Wmm1990(),
            new Wmm1995(),
            new Wmm2000(),
            new Wmm2005(),
            new Wmm2010(),
            new Wmm2015(),
            new Wmm2020(),
            new Wmm2025(),
        };

    private static IGeomagneticModel[] Igrf() =>
        IgrfModelFactory.GetModels().OrderBy(x => x.ValidFrom).ToArray();

    public static IEnumerable<object[]> AllModels() =>
        Wmm()
            .Select(x => new object[] { "WMM " + x.ValidFrom.Year, x })
            .Concat(Igrf().Select(x => new object[] { "IGRF " + x.ValidFrom.Year, x }));

    [Theory]
    [MemberData(nameof(AllModels))]
    public void Coefficients_are_zero_where_the_expansion_has_no_term(
        string name,
        IGeomagneticModel model
    )
    {
        // A spherical harmonic expansion has terms only for order m no greater than degree n,
        // the sine (h) terms vanish at m = 0, and there is no degree-0 term. A value anywhere
        // else is a coefficient written into the wrong cell.
        var problems = new List<string>();

        foreach (
            var (coefficients, label) in new[]
            {
                (model.MainCoefficientsG, "g"),
                (model.MainCoefficientsH, "h"),
                (model.SecularCoefficientsG, "gt"),
                (model.SecularCoefficientsH, "ht"),
            }
        )
            for (var n = 0; n < coefficients.GetLength(0); n++)
            for (var m = 0; m < coefficients.GetLength(1); m++)
            {
                if (coefficients[n, m] == 0)
                    continue;

                if (m > n)
                    problems.Add($"{name} {label}[{n},{m}] = {coefficients[n, m]}, but m > n");
                if (n == 0)
                    problems.Add($"{name} {label}[0,{m}] = {coefficients[n, m]}, but n = 0");
                if (label[0] == 'h' && m == 0)
                    problems.Add($"{name} {label}[{n},0] = {coefficients[n, m]}, but m = 0");
            }

        Assert.Empty(problems);
    }

    [Fact]
    public void Wmm_and_Igrf_agree_on_the_sign_of_every_coefficient_of_any_size()
    {
        // The two models differ, being separate fits, but not about which way a term points
        // once it is big enough for the difference between them to be beside the point.
        //
        // The threshold is drawn from the tables: among the coefficients whose signs the two
        // models genuinely disagree about, all are tiny - the largest is 1.0 nT on the smaller
        // side, and they sit at high degree where IGRF's older epochs are quoted to whole
        // nanoteslas. WMM2020's h[8,7] error was 6.9 nT on the smaller side, so 3 nT separates
        // the two by a factor of about seven either way.
        const double floor = 3;

        var igrf = Igrf().ToDictionary(x => x.ValidFrom.Year);
        var problems = new List<string>();

        foreach (var wmm in Wmm())
        {
            if (!igrf.TryGetValue(wmm.ValidFrom.Year, out var reference))
                continue;

            for (var n = 1; n <= 12; n++)
            for (var m = 0; m <= n; m++)
                foreach (
                    var (a, b, label) in new[]
                    {
                        (
                            wmm.MainCoefficientsG[n, m],
                            reference.MainCoefficientsG[n, m],
                            $"g[{n},{m}]"
                        ),
                        (
                            wmm.MainCoefficientsH[n, m],
                            reference.MainCoefficientsH[n, m],
                            $"h[{n},{m}]"
                        ),
                    }
                )
                    if (a * b < 0 && Math.Abs(a) > floor && Math.Abs(b) > floor)
                        problems.Add(
                            $"{wmm.ValidFrom.Year} {label}: WMM {a}, IGRF {b} - same size, opposite signs"
                        );
        }

        Assert.Empty(problems);
    }

    [Fact]
    public void Wmm_and_Igrf_agree_on_the_field_they_produce()
    {
        // A coarser companion to the sign check above, catching what that one cannot: a
        // coefficient of the right sign but the wrong size. A digit altered in the dipole
        // moves the total intensity by about 1000 nT and a misplaced decimal point by
        // several thousand, against the 289 nT the two models actually differ by at their
        // furthest apart - which is in the 1990s, where both are least well determined.
        //
        // It will not notice a sign flipped on a middling coefficient, which shifts the field
        // by under 400 nT; that is the sign check's job.
        const double tolerance = 400;

        var wmm = new WmmGeomagnetismCalculator();
        var igrf = new IgrfGeomagnetismCalculator();
        var worst = 0.0;
        var where = "";

        foreach (var year in new[] { 1986, 1992, 1997, 2002, 2007, 2012, 2017, 2022, 2027 })
        {
            var when = new DateTime(year, 6, 15, 0, 0, 0, DateTimeKind.Utc);

            for (var latitude = -80.0; latitude <= 80; latitude += 20)
            for (var longitude = -180.0; longitude <= 180; longitude += 30)
            {
                var a = wmm.TryCalculate(new Coordinate(latitude, longitude), when);
                var b = igrf.TryCalculate(new Coordinate(latitude, longitude), when);
                Assert.NotNull(a);
                Assert.NotNull(b);

                var difference = Math.Abs(a!.TotalIntensity - b!.TotalIntensity);
                if (difference > worst)
                {
                    worst = difference;
                    where = $"{year} at {latitude}, {longitude}";
                }
            }
        }

        Assert.True(
            worst <= tolerance,
            $"WMM and IGRF differ by {worst:F1} nT ({where}), beyond the {tolerance} nT the two models are expected to"
        );
    }

    [Theory]
    [InlineData("WMM")]
    [InlineData("IGRF")]
    public void Epochs_tile_their_range_without_gap_or_overlap(string family)
    {
        // GeomagnetismCalculator picks its model with SingleOrDefault, so two epochs covering
        // one instant would throw rather than answer, and a gap would report no model for a
        // date the family is meant to cover.
        var models = family == "WMM" ? Wmm() : Igrf();

        for (var i = 0; i < models.Length; i++)
        {
            Assert.True(
                models[i].ValidFrom < models[i].ValidTo,
                $"{family} {models[i].ValidFrom:yyyy} does not span any time"
            );

            if (i > 0)
                Assert.Equal(models[i - 1].ValidTo, models[i].ValidFrom);
        }

        var overlapping =
            from a in models
            from b in models
            where !ReferenceEquals(a, b) && a.ValidFrom < b.ValidTo && b.ValidFrom < a.ValidTo
            select $"{a.ValidFrom:yyyy} overlaps {b.ValidFrom:yyyy}";

        Assert.Empty(overlapping.ToArray());
    }

    [Fact]
    public void The_dipole_term_weakens_steadily_across_every_epoch()
    {
        // The Earth's dipole has been decaying throughout the modelled period, so g[1,0] -
        // by far the largest coefficient - rises monotonically towards zero from about
        // -31500 nT in 1900. A digit lost anywhere in it would break the run.
        foreach (var models in new[] { Wmm(), Igrf() })
        {
            var dipole = models.Select(x => x.MainCoefficientsG[1, 0]).ToArray();

            Assert.All(dipole, x => Assert.InRange(x, -32000, -29000));
            for (var i = 1; i < dipole.Length; i++)
                Assert.True(
                    dipole[i] > dipole[i - 1],
                    $"g[1,0] went from {dipole[i - 1]} to {dipole[i]}, against the decay"
                );
        }
    }
}
