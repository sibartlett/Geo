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
}
