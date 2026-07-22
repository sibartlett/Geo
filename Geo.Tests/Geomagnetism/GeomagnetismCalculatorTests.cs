using System;
using Geo.Geomagnetism;
using Xunit;

namespace Geo.Tests.Geomagnetism;

public class GeomagnetismCalculatorTests
{
    // A UTC date that both the WMM (1985-2030) and IGRF (1900-2015) model sets cover,
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
    public void Wmm_and_igrf_agree_on_field_direction(double lat, double lon, int year)
    {
        var date = new DateTime(year, 6, 1, 0, 0, 0, DateTimeKind.Utc);
        var coordinate = new Coordinate(lat, lon);

        var wmm = new WmmGeomagnetismCalculator().TryCalculate(coordinate, date);
        var igrf = new IgrfGeomagnetismCalculator().TryCalculate(coordinate, date);

        Assert.NotNull(wmm);
        Assert.NotNull(igrf);

        // Declination and inclination describe the direction of the field; the two
        // independent models should agree closely even though their coefficients differ.
        Assert.Equal(wmm.Declination, igrf.Declination, 0.5);
        Assert.Equal(wmm.Inclination, igrf.Inclination, 0.1);
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
    [InlineData(1899, false)] // before IGRF coverage
    [InlineData(2020, false)] // IGRF coverage in this build ends in 2015
    public void Igrf_model_selection_respects_epoch_bounds(int year, bool expected)
    {
        var success = new IgrfGeomagnetismCalculator().TryCalculate(
            new Coordinate(51.5, -0.12),
            new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            out _
        );

        Assert.Equal(expected, success);
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
