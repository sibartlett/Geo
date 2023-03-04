using System;

namespace Geo.Geomagnetism;

// From http://stackoverflow.com/questions/5248827/convert-datetime-to-julian-date-in-c-sharp-tooadate-safe
internal class JulianDate
{
    public static bool IsJulianDate(int year, int month, int day)
    {
        // All dates prior to 1582 are in the Julian calendar
        if (year < 1582)
            return true;

        // All dates after 1582 are in the Gregorian calendar
        if (year > 1582)
            return false;

        // If 1582, check before October 4 (Julian) or after October 15 (Gregorian)
        if (month < 10)
            return true;
        if (month > 10)
            return false;

        if (day < 5)
            return true;
        if (day > 14)
            return false;

        // Any date in the range 10/5/1582 to 10/14/1582 is invalid 
        throw new NotSupportedException(
            "Date is not valid as it does not exist in either the Julian or the Gregorian calendars.");
    }

    private static double DateToJD(int year, int month, int day, int hour, int minute, int second, int millisecond)
    {
        // Determine correct calendar based on date
        var isJulianDate = IsJulianDate(year, month, day);

        var m = month > 2 ? month : month + 12;
        var y = month > 2 ? year : year - 1;
        var d = day + hour / 24.0 + minute / 1440.0 + (second + millisecond * 1000) / 86400.0;
        var b = isJulianDate ? 0 : 2 - y / 100 + y / 100 / 4;

        return (int)(365.25 * (y + 4716)) + (int)(30.6001 * (m + 1)) + d + b - 1524.5;
    }

    public static double JD(int year, int month, int day, int hour, int minute, int second, int millisecond)
    {
        return DateToJD(year, month, day, hour, minute, second, millisecond);
    }

    public static double JD(DateTime date)
    {
        return DateToJD(date.Year, date.Month, date.Day, date.Hour, date.Minute, date.Second, date.Millisecond);
    }
}