namespace Geo.Gps;

public static class GpsFeaturesExtensions
{
    /// <summary>
    /// Determines whether <paramref name="supportedFeatures" /> includes <em>all</em> of the
    /// features in <paramref name="features" />. For a combined request such as
    /// <see cref="GpsFeatures.TracksAndWaypoints" /> every requested flag must be present, so a
    /// format that supports only one of them is not a match. This mirrors the semantics of
    /// <see cref="System.Enum.HasFlag" />.
    /// </summary>
    public static bool Contains(this GpsFeatures supportedFeatures, GpsFeatures features)
    {
        return (supportedFeatures & features) == features;
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
