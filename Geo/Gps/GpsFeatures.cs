using System;

namespace Geo.Gps
{
    [Flags]
    public enum GpsFeatures
    {
        Routes = 1,
        Tracks = 2,
        Waypoints = 4,

        RoutesAndTracks = 3,
        RoutesAndWaypoints = 5,
        TracksAndWaypoints = 6,

        All = 7,
    }
}