#nullable enable
using System;

namespace Geo.Geodesy;

public class Spheroid
{
    public static readonly Spheroid Default = Wgs84;

    private readonly double _authalicQAtPole;

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

        _authalicQAtPole = AuthalicQ(1);
        AuthalicRadius = EquatorialAxis * Math.Sqrt(_authalicQAtPole / 2);
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

    /// <summary>
    /// The radius of the sphere with the same surface area as this spheroid (6371007.181 m
    /// for WGS-84). An area worked out on that sphere is the spheroid's own area, which is
    /// what makes it the sphere to reduce an area calculation to - <see cref="MeanRadius" />
    /// is an average of the axes and carries no such guarantee.
    /// </summary>
    public double AuthalicRadius { get; }

    /// <summary>
    /// The sine of the authalic latitude: the latitude on the sphere of
    /// <see cref="AuthalicRadius" /> that has the same area between it and the equator as
    /// <paramref name="latitude" /> does here. Substituting it for the sine of the true
    /// latitude turns a spherical area formula into a spheroidal one.
    /// </summary>
    internal double AuthalicSine(double latitude)
    {
        return AuthalicQ(Math.Sin(latitude.ToRadians())) / _authalicQAtPole;
    }

    /// <summary>
    /// The area, in units of the equatorial radius squared, between the equator and the
    /// latitude whose sine is <paramref name="sinLatitude" />, per radian of longitude
    /// (Snyder's <em>q</em>).
    /// </summary>
    private double AuthalicQ(double sinLatitude)
    {
        var eccentricity = Eccentricity;

        // A sphere's own latitude is already authalic, and the expression below is 0/0
        // there, so take the limit it tends to.
        if (eccentricity < 1e-12)
            return 2 * sinLatitude;

        var eccentricitySquared = eccentricity * eccentricity;
        return (1 - eccentricitySquared)
            * (
                sinLatitude / (1 - eccentricitySquared * sinLatitude * sinLatitude)
                - Math.Log((1 - eccentricity * sinLatitude) / (1 + eccentricity * sinLatitude))
                    / (2 * eccentricity)
            );
    }
}
