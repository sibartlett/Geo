using Geo.Geodesy;
using Xunit;

namespace Geo.Tests.Geodesy;

public class SpheroidTests
{
    [Fact]
    public void Constructor_derives_flattening_axes_and_radius()
    {
        var spheroid = new Spheroid("WGS84", 6378137d, 298.257223563d);

        Assert.Equal("WGS84", spheroid.Name);
        Assert.Equal(6378137d, spheroid.EquatorialAxis);
        Assert.Equal(298.257223563d, spheroid.InverseFlattening);
        Assert.Equal(1 / 298.257223563d, spheroid.Flattening, 15);
        Assert.Equal(6378137d * (1 - 1 / 298.257223563d), spheroid.PolarAxis, 9);
        Assert.Equal(
            (2 * spheroid.EquatorialAxis + spheroid.PolarAxis) / 3,
            spheroid.MeanRadius,
            9
        );
        Assert.False(spheroid.IsSphere);
    }

    [Fact]
    public void A_spheroid_with_no_flattening_is_a_sphere()
    {
        // A very large inverse flattening makes the polar and equatorial axes coincide.
        var spheroid = new Spheroid("Sphere", 6371000d, double.PositiveInfinity);

        Assert.True(spheroid.IsSphere);
        Assert.Equal(spheroid.EquatorialAxis, spheroid.PolarAxis);
        Assert.Equal(0, spheroid.Flattening);
    }

    [Theory]
    [InlineData("WGS84", 6378137d, 298.257223563d)]
    [InlineData("GRS80", 6378137d, 298.257222101d)]
    [InlineData("International 1924", 6378388d, 297d)]
    [InlineData("Clarke 1866", 6378206.4d, 294.9786982d)]
    public void Named_spheroids_expose_their_defining_parameters(
        string name,
        double equatorialAxis,
        double inverseFlattening
    )
    {
        var spheroid = name switch
        {
            "WGS84" => Spheroid.Wgs84,
            "GRS80" => Spheroid.Grs80,
            "International 1924" => Spheroid.International1924,
            _ => Spheroid.Clarke1866,
        };

        Assert.Equal(name, spheroid.Name);
        Assert.Equal(equatorialAxis, spheroid.EquatorialAxis);
        Assert.Equal(inverseFlattening, spheroid.InverseFlattening);
        Assert.False(spheroid.IsSphere);
    }

    [Fact]
    public void Default_spheroid_is_wgs84()
    {
        Assert.Equal(Spheroid.Wgs84.Name, Spheroid.Default.Name);
        Assert.Equal(Spheroid.Wgs84.EquatorialAxis, Spheroid.Default.EquatorialAxis);
    }

    [Fact]
    public void Authalic_radius_is_the_published_value()
    {
        // The equal-area radius of WGS-84 is a published constant (R2 in NIMA TR8350.2).
        Assert.Equal(6371007.181, Spheroid.Wgs84.AuthalicRadius, 1e-3);
    }

    [Fact]
    public void Authalic_radius_of_a_sphere_is_its_own_radius()
    {
        var sphere = new Spheroid("Sphere", 6371000d, double.PositiveInfinity);

        Assert.Equal(6371000d, sphere.AuthalicRadius, 1e-6);
    }

    [Theory]
    [InlineData(298.257223563)]
    [InlineData(297)]
    [InlineData(294.9786982)]
    [InlineData(50)]
    public void Authalic_radius_lies_between_the_two_axes(double inverseFlattening)
    {
        // An equal-area sphere has to sit between the flattened and unflattened extremes.
        var spheroid = new Spheroid("test", 6378137d, inverseFlattening);

        Assert.InRange(spheroid.AuthalicRadius, spheroid.PolarAxis, spheroid.EquatorialAxis);
    }

    [Fact]
    public void Authalic_radius_scales_with_the_spheroid()
    {
        var full = new Spheroid("full", 6378137d, 298.257223563d);
        var half = new Spheroid("half", 6378137d / 2, 298.257223563d);

        Assert.Equal(full.AuthalicRadius / 2, half.AuthalicRadius, 1e-6);
    }
}
