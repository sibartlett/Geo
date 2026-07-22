using System;
using Geo.Geomagnetism;
using Xunit;

namespace Geo.Tests.Geomagnetism;

public class JulianDateTests
{
    [Theory]
    [InlineData(1500, 6, 1, true)] // well before the reform: Julian
    [InlineData(1581, 12, 31, true)] // year before 1582: Julian
    [InlineData(1582, 9, 30, true)] // 1582 before October: Julian
    [InlineData(1582, 10, 4, true)] // last Julian day
    [InlineData(1582, 10, 15, false)] // first Gregorian day
    [InlineData(1582, 11, 1, false)] // 1582 after October: Gregorian
    [InlineData(1583, 1, 1, false)] // year after 1582: Gregorian
    [InlineData(2000, 1, 1, false)] // modern: Gregorian
    public void IsJulianDate_classifies_dates_around_the_1582_reform(
        int year,
        int month,
        int day,
        bool expected
    )
    {
        Assert.Equal(expected, JulianDate.IsJulianDate(year, month, day));
    }

    [Theory]
    [InlineData(1582, 10, 5)]
    [InlineData(1582, 10, 10)]
    [InlineData(1582, 10, 14)]
    public void IsJulianDate_throws_for_the_nonexistent_reform_gap(int year, int month, int day)
    {
        Assert.Throws<NotSupportedException>(() => JulianDate.IsJulianDate(year, month, day));
    }

    [Fact]
    public void JD_matches_the_J2000_epoch()
    {
        // 2000-01-01 12:00:00 UT is the canonical Julian Date 2451545.0.
        Assert.Equal(2451545.0, JulianDate.JD(2000, 1, 1, 12, 0, 0, 0), 6);
    }

    [Fact]
    public void JD_advances_by_one_per_day()
    {
        var day1 = JulianDate.JD(2000, 1, 1, 12, 0, 0, 0);
        var day2 = JulianDate.JD(2000, 1, 2, 12, 0, 0, 0);

        Assert.Equal(1.0, day2 - day1, 6);
    }

    [Fact]
    public void JD_is_continuous_across_the_1582_reform()
    {
        // The last Julian day and the first Gregorian day are consecutive.
        var lastJulian = JulianDate.JD(1582, 10, 4, 0, 0, 0, 0);
        var firstGregorian = JulianDate.JD(1582, 10, 15, 0, 0, 0, 0);

        Assert.Equal(2299159.5, lastJulian, 6);
        Assert.Equal(2299160.5, firstGregorian, 6);
        Assert.Equal(1.0, firstGregorian - lastJulian, 6);
    }

    [Fact]
    public void JD_datetime_overload_matches_the_component_overload()
    {
        var date = new DateTime(2012, 6, 15, 6, 30, 0, DateTimeKind.Utc);
        var viaDateTime = JulianDate.JD(date);
        var viaComponents = JulianDate.JD(2012, 6, 15, 6, 30, 0, 0);

        Assert.Equal(viaComponents, viaDateTime, 9);
    }
}
