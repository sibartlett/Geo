using System;
using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Gps;

public class Waypoint : IHasLength
{
    public Waypoint(double latitude, double longitude)
    {
        Point = new Point(latitude, longitude);
    }

    public Waypoint(double latitude, double longitude, double elevation)
    {
        Point = new Point(latitude, longitude, elevation);
    }

    public Waypoint(double latitude, double longitude, double elevation, DateTime dateTime)
    {
        Point = new Point(latitude, longitude, elevation);
        TimeUtc = dateTime;
    }

    public Waypoint(Point point, DateTime dateTime)
    {
        Point = point;
        TimeUtc = dateTime;
    }

    public Waypoint(Point point, string name, string comment, string description)
    {
        Name = name;
        Comment = comment;
        Description = description;
        Point = point;
    }

    public Waypoint(
        Point point,
        DateTime? dateTime,
        string name,
        string comment,
        string description
    )
    {
        Name = name;
        Comment = comment;
        Description = description;
        Point = point;
        TimeUtc = dateTime;
    }

    public string Name { get; }
    public string Comment { get; }
    public string Description { get; }

    public Point Point { get; set; }
    public DateTime? TimeUtc { get; set; }

    public Coordinate Coordinate => Point.Coordinate;

    public Distance GetLength()
    {
        return ToLineString().GetLength();
    }

    public LineString ToLineString()
    {
        return new LineString(Point.Coordinate);
    }
}
