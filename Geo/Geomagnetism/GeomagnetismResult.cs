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

    /// <summary>
    /// The northerly intensity of the magnetic field, in nanoteslas.
    /// This is the north component of the NED (North, East, Down) geodetic
    /// reference frame used by the World Magnetic Model. To convert to an
    /// ENU (East, North, Up) frame, use (East, North, Up) = (<see cref="Y" />,
    /// <see cref="X" />, -<see cref="Z" />).
    /// </summary>
    public double X { get; }

    /// <summary>
    /// The easterly intensity of the magnetic field, in nanoteslas.
    /// This is the east component of the NED (North, East, Down) geodetic
    /// reference frame used by the World Magnetic Model.
    /// </summary>
    public double Y { get; }

    /// <summary>
    /// The vertical (downward) intensity of the magnetic field, in nanoteslas.
    /// This is the down component of the NED (North, East, Down) geodetic
    /// reference frame used by the World Magnetic Model, so positive values
    /// point towards the centre of the Earth.
    /// </summary>
    public double Z { get; }

    /// <summary>
    /// The magnetic declination (D) in degrees: the angle between magnetic
    /// north and true (geographic) north, measured positive eastwards.
    /// </summary>
    public double Declination { get; }

    /// <summary>
    /// The magnetic inclination (I), or dip angle, in degrees: the angle
    /// between the horizontal plane and the magnetic field vector, measured
    /// positive downwards.
    /// </summary>
    public double Inclination { get; }

    /// <summary>
    /// The total intensity (F) of the magnetic field, in nanoteslas.
    /// </summary>
    public double TotalIntensity { get; }

    /// <summary>
    /// The horizontal intensity (H) of the magnetic field, in nanoteslas.
    /// </summary>
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
