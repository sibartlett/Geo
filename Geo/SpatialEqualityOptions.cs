namespace Geo
{
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
    }
}
