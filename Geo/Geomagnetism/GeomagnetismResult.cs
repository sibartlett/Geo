using System;
using System.Globalization;

namespace Geo.Geomagnetism;

public class GeomagnetismResult
{
    public GeomagnetismResult(CoordinateZ coordinate, DateTime date, double x, double y, double z)
    {
        Date = date;
        Coordinate = coordinate;
        if (Math.Abs(x - 0.0) < double.Epsilon * 2 || Math.Abs(y - 0.0) < double.Epsilon * 2)
            return;

        X = x;
        Y = y;
        Z = z;

        HorizontalIntensity = Math.Sqrt(x * x + y * y);
        TotalIntensity = Math.Sqrt(x * x + y * y + z * z);
        Declination = Math.Atan2(y, x).ToDegrees();
        Inclination = Math.Atan2(z, HorizontalIntensity).ToDegrees();
    }

    public CoordinateZ Coordinate { get; }
    public DateTime Date { get; }

    public double X { get; }
    public double Y { get; }
    public double Z { get; }
    public double Declination { get; }
    public double Inclination { get; }
    public double TotalIntensity { get; }
    public double HorizontalIntensity { get; }

    public override string ToString()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "Magnetic Field[D={0}, I={1}, H={2}, F={3}, X={4}, Y={5}, Z={6}]",
            Declination,
            Inclination,
            HorizontalIntensity,
            TotalIntensity,
            X,
            Y,
            Z
        );
    }
}
