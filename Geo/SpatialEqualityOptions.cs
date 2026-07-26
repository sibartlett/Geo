#nullable enable
namespace Geo;

public class SpatialEqualityOptions
{
    public SpatialEqualityOptions()
    {
        UseElevation = true;
        UseM = true;
        PoleCoordiantesAreEqual = true;
        AntiMeridianCoordinatesAreEqual = true;
    }

    /// <summary>
    /// The options every parameterless <c>GetHashCode</c> hashes under, so that a hash
    /// never depends on which options happen to be in force. Only the ordinates a position
    /// always has are hashed; the elevation and the measure are left out, because whether
    /// they count towards equality is exactly what varies.
    /// </summary>
    /// <remarks>
    /// Shared and never copied, so nothing may write to it. It is not exposed outside the
    /// assembly for that reason.
    /// </remarks>
    internal static readonly SpatialEqualityOptions PositionOnly = new()
    {
        UseElevation = false,
        UseM = false,
    };

    public bool UseElevation { get; set; }
    public bool UseM { get; set; }
    public bool PoleCoordiantesAreEqual { get; set; }
    public bool AntiMeridianCoordinatesAreEqual { get; set; }

    private SpatialEqualityOptions Transform(bool elevation)
    {
        return new SpatialEqualityOptions
        {
            AntiMeridianCoordinatesAreEqual = AntiMeridianCoordinatesAreEqual,
            PoleCoordiantesAreEqual = PoleCoordiantesAreEqual,
            UseElevation = elevation,
            UseM = false,
        };
    }

    public SpatialEqualityOptions To2D()
    {
        return Transform(false);
    }

    public SpatialEqualityOptions To3D()
    {
        return Transform(true);
    }
}
