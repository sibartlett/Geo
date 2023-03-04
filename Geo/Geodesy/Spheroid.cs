using System;

namespace Geo.Geodesy;

public class Spheroid
{
    public static readonly Spheroid Default = Wgs84;

    public Spheroid(string name, double equatorialAxis, double inverseFlattening)
    {
        Name = name;
        InverseFlattening = inverseFlattening;
        Flattening = 1 / inverseFlattening;
        EquatorialAxis = equatorialAxis;
        PolarAxis = equatorialAxis * (1 - 1 / inverseFlattening);
        Eccentricity = Math.Sqrt(2 * Flattening - Flattening * Flattening);
        MeanRadius = (2 * EquatorialAxis + PolarAxis) / 3;
        IsSphere = Math.Abs(EquatorialAxis - PolarAxis) < double.Epsilon;
    }

    public static Spheroid Wgs84 => new("WGS84", 6378137d, 298.257223563d);

    public static Spheroid Grs80 => new("GRS80", 6378137d, 298.257222101);

    public static Spheroid International1924 => new("International 1924", 6378388d, 297d);

    public static Spheroid Clarke1866 => new("Clarke 1866", 6378206.4, 294.9786982);

    public string Name { get; }
    public double Flattening { get; }
    public double InverseFlattening { get; }
    public double EquatorialAxis { get; }
    public double PolarAxis { get; }
    public double Eccentricity { get; }
    public double MeanRadius { get; }
    public bool IsSphere { get; }
}