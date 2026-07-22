using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps;

public class RouteTests
{
    [Fact]
    public void A_new_route_is_empty_with_metadata()
    {
        var route = new Route();

        Assert.NotNull(route.Metadata);
        Assert.NotNull(route.Waypoints);
        Assert.Empty(route.Waypoints);
    }

    [Fact]
    public void ToLineString_carries_every_waypoint_coordinate()
    {
        var route = new Route();
        route.Waypoints.Add(new Waypoint(0, 0));
        route.Waypoints.Add(new Waypoint(0, 1));
        route.Waypoints.Add(new Waypoint(0, 2));

        var lineString = route.ToLineString();

        Assert.Equal(3, lineString.Coordinates.Count);
        Assert.Equal(route.Waypoints[0].Coordinate, lineString.Coordinates[0]);
        Assert.Equal(route.Waypoints[2].Coordinate, lineString.Coordinates[2]);
    }

    [Fact]
    public void Length_is_positive_and_matches_the_line_string()
    {
        var route = new Route();
        route.Waypoints.Add(new Waypoint(0, 0));
        route.Waypoints.Add(new Waypoint(0, 1));

        Assert.True(route.GetLength().SiValue > 0);
        Assert.Equal(route.ToLineString().GetLength().SiValue, route.GetLength().SiValue, 1e-6);
    }
}
