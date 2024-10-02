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
