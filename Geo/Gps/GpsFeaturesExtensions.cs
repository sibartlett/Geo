namespace Geo.Gps
{
    public static class GpsFeaturesExtensions
    {
        public static bool Contains(this GpsFeatures supportedFeatures, GpsFeatures features)
        {
            return (supportedFeatures & features) != 0;
        }
        
        public static bool Routes(this GpsFeatures supportedFeatures)
        {
            return (supportedFeatures & GpsFeatures.Routes) != 0;
        }
        
        public static bool Tracks(this GpsFeatures supportedFeatures)
        {
            return (supportedFeatures & GpsFeatures.Tracks) != 0;
        }

        public static bool Waypoints(this GpsFeatures supportedFeatures)
        {
            return (supportedFeatures & GpsFeatures.Waypoints) != 0;
        }
    }
}