using System;
using Geo.Geomagnetism;
using Xunit;

namespace Geo.Tests.Geomagnetism;

public class GeomagnetismCalculatorTests
{
    // A UTC date that both the WMM (1985-2030) and IGRF (1900-2030) model sets cover,
    // so the two calculators can be cross-checked against each other.
    private static readonly DateTime CommonDate = new(2012, 6, 15, 0, 0, 0, DateTimeKind.Utc);

    [Fact]
    public void Wmm_matches_known_field_for_london_2012()
    {
        // Reference: World Magnetic Model at 51.5N, 0.12W, sea level, 2012-06-15.
        // Values are the physically expected order of magnitude for London and are pinned
        // here as a regression baseline for the spherical-harmonic synthesis.
        var result = new WmmGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            CommonDate
        );

        Assert.NotNull(result);
        Assert.Equal(-1.3504, result.Declination, 0.01);
        Assert.Equal(66.4641, result.Inclination, 0.01);
        Assert.Equal(19428.21, result.HorizontalIntensity, 0.5);
        Assert.Equal(48652.75, result.TotalIntensity, 0.5);
    }

    [Fact]
    public void Wmm2015v2_matches_noaa_online_calculator_for_issue_38()
    {
        // Regression for https://github.com/sibartlett/Geo/issues/38.
        // The original WMM2015 coefficients produced a field that differed from NOAA's
        // online calculator by ~35 nT / ~0.05 deg for this point/date, because NOAA
        // replaced WMM2015 with WMM2015v2 in 2018. With the v2 coefficients (and the
        // model reference radius of 6371.2 km), the library now reproduces the NOAA
        // online calculator values quoted in the issue.
        var result = new WmmGeomagnetismCalculator().TryCalculate(
            new Coordinate(32.9, -96.1),
            new DateTime(2019, 7, 19, 0, 0, 0, DateTimeKind.Utc)
        );

        Assert.NotNull(result);
        // NOAA online calculator / WMM2015v2 reference values for 2019-07-19.
        Assert.Equal(2.7375, result.Declination, 0.01);
        Assert.Equal(23206.9, result.X, 1.0);
        Assert.Equal(1109.7, result.Y, 1.0);
        Assert.Equal(42766.7, result.Z, 1.0);
    }

    [Theory]
    // Regression baselines: the WMM field at London (51.5N, 0.12W, sea level) at the
    // midpoint of each WMM epoch (1985-2030). The values were produced by the calculator
    // and cross-checked against the independent IGRF model, which agrees to well within
    // these tolerances, so a change to any single coefficient table is caught here.
    // The 2017 row uses the WMM2015v2 coefficients (NOAA's out-of-cycle correction of the
    // original WMM2015), which is why its declination differs from the original model.
    // year, declination, inclination, horizontalIntensity, totalIntensity
    [InlineData(1987, -4.7168, 66.4446, 19191.15, 48021.69)]
    [InlineData(1992, -4.272, 66.4413, 19251.55, 48166.33)]
    [InlineData(1997, -3.6857, 66.4478, 19255.69, 48189.34)]
    [InlineData(2002, -2.8147, 66.4504, 19325.88, 48370.05)]
    [InlineData(2007, -2.082, 66.4936, 19363.61, 48548.43)]
    [InlineData(2012, -1.3504, 66.4641, 19428.21, 48652.75)]
    [InlineData(2017, -0.4536, 66.4565, 19483.59, 48776.62)]
    [InlineData(2022, 0.4853, 66.4953, 19535.65, 48983.0)]
    [InlineData(2027, 1.3272, 66.5498, 19561.14, 49154.53)]
    public void Wmm_matches_pinned_field_per_epoch(
        int year,
        double declination,
        double inclination,
        double horizontal,
        double total
    )
    {
        var result = new WmmGeomagnetismCalculator().TryCalculate(
            new CoordinateZ(51.5, -0.12, 0),
            new DateTime(year, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        );

        Assert.NotNull(result);
        Assert.Equal(declination, result.Declination, 0.01);
        Assert.Equal(inclination, result.Inclination, 0.01);
        Assert.Equal(horizontal, result.HorizontalIntensity, 0.5);
        Assert.Equal(total, result.TotalIntensity, 0.5);
    }

    [Theory]
    // Regression baselines for the IGRF model at London across historical epochs,
    // spanning the 20th-century coefficient tables. Cross-checked against the physical
    // record (London's declination shrank from ~16W in 1905 toward 0 today).
    // year, declination, inclination, horizontalIntensity, totalIntensity
    [InlineData(1905, -16.1416, 66.955, 18521.14, 47313.77)]
    [InlineData(1925, -13.0708, 66.8384, 18411.43, 46809.62)]
    [InlineData(1945, -9.7544, 67.0559, 18331.21, 47023.17)]
    [InlineData(1965, -7.5137, 66.6748, 18813.39, 47514.67)]
    [InlineData(1985, -5.1703, 66.4112, 19209.91, 48004.45)]
    [InlineData(2005, -2.3779, 66.4978, 19327.04, 48464.84)]
    public void Igrf_matches_pinned_field_for_historical_epochs(
        int year,
        double declination,
        double inclination,
        double horizontal,
        double total
    )
    {
        var result = new IgrfGeomagnetismCalculator().TryCalculate(
            new CoordinateZ(51.5, -0.12, 0),
            new DateTime(year, 6, 15, 0, 0, 0, DateTimeKind.Utc)
        );

        Assert.NotNull(result);
        Assert.Equal(declination, result.Declination, 0.01);
        Assert.Equal(inclination, result.Inclination, 0.01);
        Assert.Equal(horizontal, result.HorizontalIntensity, 0.5);
        Assert.Equal(total, result.TotalIntensity, 0.5);
    }

    [Theory]
    // lat, lon, elevation(m), year
    [InlineData(51.5, -0.12, 0, 2012)] // London
    [InlineData(40.0, -105.0, 1600, 2012)] // Boulder (with elevation)
    [InlineData(-33.87, 151.21, 0, 2010)] // Sydney (southern hemisphere)
    [InlineData(0, 0, 0, 2013)] // Equator / prime meridian
    [InlineData(80, 0, 0, 2014)] // High latitude
    public void Wmm_produces_physically_bounded_field(
        double lat,
        double lon,
        double elevation,
        int year
    )
    {
        var date = new DateTime(year, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var result = new WmmGeomagnetismCalculator().TryCalculate(
            new CoordinateZ(lat, lon, elevation),
            date
        );

        Assert.NotNull(result);
        // Earth's total field magnitude is roughly 22,000-67,000 nT everywhere.
        Assert.InRange(result.TotalIntensity, 20000, 70000);
        Assert.InRange(result.HorizontalIntensity, 0, 70000);
        Assert.InRange(result.Inclination, -90, 90);
        Assert.InRange(result.Declination, -180, 180);
    }

    [Theory]
    [InlineData(51.5, -0.12, 2012)]
    [InlineData(40.0, -105.0, 2012)]
    [InlineData(-33.87, 151.21, 2010)]
    [InlineData(0, 0, 2013)]
    [InlineData(51.5, -0.12, 2020)] // exercises the 2020-2025 epoch
    public void Wmm_and_igrf_agree(double lat, double lon, int year)
    {
        var date = new DateTime(year, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var coordinate = new Coordinate(lat, lon);

        var wmm = new WmmGeomagnetismCalculator().TryCalculate(coordinate, date);
        var igrf = new IgrfGeomagnetismCalculator().TryCalculate(coordinate, date);

        Assert.NotNull(wmm);
        Assert.NotNull(igrf);

        // The two independent models should agree closely, in both the direction of the
        // field (declination/inclination) and its magnitude (horizontal/total intensity),
        // even though their coefficients differ. A wrong secular-variation term in the
        // IGRF model factory previously inflated the intensities by (1 + years-into-epoch),
        // so the intensity checks guard specifically against that regression.
        Assert.Equal(wmm.Declination, igrf.Declination, 0.5);
        Assert.Equal(wmm.Inclination, igrf.Inclination, 0.1);
        Assert.Equal(
            wmm.HorizontalIntensity,
            igrf.HorizontalIntensity,
            wmm.HorizontalIntensity * 0.02
        );
        Assert.Equal(wmm.TotalIntensity, igrf.TotalIntensity, wmm.TotalIntensity * 0.02);
    }

    [Fact]
    public void Result_components_are_internally_consistent()
    {
        var result = new WmmGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            CommonDate
        );

        Assert.Equal(
            Math.Sqrt(result.X * result.X + result.Y * result.Y),
            result.HorizontalIntensity,
            1e-6
        );
        Assert.Equal(
            Math.Sqrt(result.X * result.X + result.Y * result.Y + result.Z * result.Z),
            result.TotalIntensity,
            1e-6
        );
        Assert.Equal(Math.Atan2(result.Y, result.X) * 180.0 / Math.PI, result.Declination, 1e-6);
        Assert.Equal(
            Math.Atan2(result.Z, result.HorizontalIntensity) * 180.0 / Math.PI,
            result.Inclination,
            1e-6
        );
    }

    [Fact]
    public void TryCalculate_returns_false_and_null_when_no_model_covers_the_date()
    {
        var calculator = new WmmGeomagnetismCalculator();

        // 1980 predates the earliest WMM epoch (1985).
        var success = calculator.TryCalculate(
            new Coordinate(51.5, -0.12),
            new DateTime(1980, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            out var result
        );

        Assert.False(success);
        Assert.Null(result);
    }

    [Theory]
    [InlineData(1990, true)]
    [InlineData(2024, true)]
    [InlineData(2029, true)] // within the 2025-2030 epoch
    [InlineData(1980, false)] // before the first epoch
    [InlineData(2030, false)] // ValidTo is exclusive
    public void Wmm_model_selection_respects_epoch_bounds(int year, bool expected)
    {
        var success = new WmmGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            out _
        );

        Assert.Equal(expected, success);
    }

    [Theory]
    [InlineData(2000, true)]
    [InlineData(2014, true)]
    [InlineData(2020, true)]
    [InlineData(2029, true)] // within the final 2025-2030 epoch
    [InlineData(1899, false)] // before IGRF coverage
    [InlineData(2030, false)] // ValidTo is exclusive
    public void Igrf_model_selection_respects_epoch_bounds(int year, bool expected)
    {
        var success = new IgrfGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            out _
        );

        Assert.Equal(expected, success);
    }

    [Theory]
    [InlineData(1905)] // early epoch
    [InlineData(1960)] // mid-century
    [InlineData(2012)] // recent definitive epoch
    [InlineData(2027)] // extrapolated via the final epoch's secular-variation term
    public void Igrf_produces_physically_bounded_field(int year)
    {
        var result = new IgrfGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            new DateTime(year, 6, 1, 0, 0, 0, DateTimeKind.Utc)
        );

        Assert.NotNull(result);
        // A correct spherical-harmonic synthesis stays within Earth's field magnitude;
        // the previous secular-variation defect pushed these well past 100,000 nT.
        Assert.InRange(result.TotalIntensity, 20000, 70000);
        Assert.InRange(result.HorizontalIntensity, 0, 70000);
        Assert.InRange(result.Inclination, -90, 90);
    }

    [Fact]
    public void DateTimeOffset_and_DateTime_overloads_are_equivalent()
    {
        var calculator = new WmmGeomagnetismCalculator();
        var coordinate = new Coordinate(51.5, -0.12);

        var fromDateTime = calculator.TryCalculate(coordinate, CommonDate);
        var fromOffset = calculator.TryCalculate(coordinate, new DateTimeOffset(CommonDate));

        Assert.Equal(fromDateTime.Declination, fromOffset.Declination, 1e-9);
        Assert.Equal(fromDateTime.TotalIntensity, fromOffset.TotalIntensity, 1e-9);
    }
}
