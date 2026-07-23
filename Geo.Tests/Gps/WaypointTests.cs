using System;
using Geo.Geometries;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps;

public class WaypointTests
{
    [Fact]
    public void Lat_lon_constructor_places_a_2d_point()
    {
        var waypoint = new Waypoint(1, 2);

        Assert.Equal(new Coordinate(1, 2), waypoint.Coordinate);
        Assert.False(waypoint.Point.Is3D);
        Assert.Null(waypoint.TimeUtc);
    }

    [Fact]
    public void Lat_lon_elevation_constructor_places_a_3d_point()
    {
        var waypoint = new Waypoint(1, 2, 300);

        Assert.True(waypoint.Point.Is3D);
        Assert.Equal(new CoordinateZ(1, 2, 300), waypoint.Coordinate);
    }

    [Fact]
    public void Lat_lon_elevation_time_constructor_records_the_timestamp()
    {
        var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var waypoint = new Waypoint(1, 2, 300, time);

        Assert.Equal(new CoordinateZ(1, 2, 300), waypoint.Coordinate);
        Assert.Equal(time, waypoint.TimeUtc);
    }

    [Fact]
    public void Point_and_time_constructor_keeps_the_supplied_point()
    {
        var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var point = new Point(1, 2, 300);
        var waypoint = new Waypoint(point, time);

        Assert.Same(point, waypoint.Point);
        Assert.Equal(time, waypoint.TimeUtc);
    }

    [Fact]
    public void Point_with_metadata_constructor_exposes_name_comment_and_description()
    {
        var waypoint = new Waypoint(new Point(1, 2), "name", "comment", "description");

        Assert.Equal("name", waypoint.Name);
        Assert.Equal("comment", waypoint.Comment);
        Assert.Equal("description", waypoint.Description);
        Assert.Null(waypoint.TimeUtc);
    }

    [Fact]
    public void Full_constructor_captures_time_and_metadata()
    {
        var time = new DateTime(2020, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var waypoint = new Waypoint(new Point(1, 2), time, "name", "comment", "description");

        Assert.Equal("name", waypoint.Name);
        Assert.Equal("comment", waypoint.Comment);
        Assert.Equal("description", waypoint.Description);
        Assert.Equal(time, waypoint.TimeUtc);
    }

    [Fact]
    public void ToLineString_wraps_the_single_coordinate()
    {
        var lineString = new Waypoint(1, 2).ToLineString();

        Assert.Single(lineString.Coordinates);
        Assert.Equal(new Coordinate(1, 2), lineString.Coordinates[0]);
    }

    [Fact]
    public void GetLength_of_a_single_point_is_zero()
    {
        Assert.Equal(0, new Waypoint(1, 2).GetLength().SiValue);
    }
}
