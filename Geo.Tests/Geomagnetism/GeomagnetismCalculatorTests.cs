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
        Assert.Equal(-1.3507, result.Declination, 0.01);
        Assert.Equal(66.4638, result.Inclination, 0.01);
        Assert.Equal(19426.71, result.HorizontalIntensity, 0.5);
        Assert.Equal(48648.51, result.TotalIntensity, 0.5);
    }

    [Theory]
    // Regression baselines: the WMM field at London (51.5N, 0.12W, sea level) at the
    // midpoint of each WMM epoch (1985-2030). The values were produced by the calculator
    // and cross-checked against the independent IGRF model, which agrees to well within
    // these tolerances, so a change to any single coefficient table is caught here.
    // year, declination, inclination, horizontalIntensity, totalIntensity
    [InlineData(1987, -4.7171, 66.4444, 19189.68, 48017.59)]
    [InlineData(1992, -4.2724, 66.4411, 19250.06, 48162.19)]
    [InlineData(1997, -3.686, 66.4476, 19254.21, 48185.19)]
    [InlineData(2002, -2.815, 66.4502, 19324.39, 48365.86)]
    [InlineData(2007, -2.0823, 66.4934, 19362.12, 48544.22)]
    [InlineData(2012, -1.3507, 66.4638, 19426.71, 48648.51)]
    [InlineData(2017, -0.518, 66.4508, 19475.72, 48745.89)]
    [InlineData(2022, 0.4849, 66.495, 19534.14, 48978.71)]
    [InlineData(2027, 1.3268, 66.5495, 19559.63, 49150.21)]
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
    [InlineData(1905, -16.1415, 66.9549, 18519.73, 47309.91)]
    [InlineData(1925, -13.0709, 66.8382, 18410.03, 46805.76)]
    [InlineData(1945, -9.7546, 67.0557, 18329.84, 47019.26)]
    [InlineData(1965, -7.514, 66.6746, 18811.95, 47510.68)]
    [InlineData(1985, -5.1706, 66.411, 19208.43, 48000.35)]
    [InlineData(2005, -2.3782, 66.4975, 19325.55, 48460.64)]
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
