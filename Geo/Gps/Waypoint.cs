using Geo.Abstractions.Interfaces;
using Geo.Geometries;
using Geo.Measure;

namespace Geo.Gps
{
    public class Waypoint : IRavenIndexable, IHasLength
    {
        public string Name { get; private set; }
        public string Comment { get; private set; }
        public string Description { get; private set; }

        public Point Point { get; set; }

        public Waypoint(string name, string comment, string description, Point point)
        {
            Name = name;
            Comment = comment;
            Description = description;
            Point = point;
        }

        public Coordinate Coordinate
        {
            get { return Point.Coordinate; }
        }

        public LineString ToLineString()
        {
            return new LineString(Point.Coordinate);
        }

        ISpatial4nShape IRavenIndexable.GetSpatial4nShape()
        {
            return ToLineString();
        }

        public Distance GetLength()
        {
            return ToLineString().GetLength();
        }
    }
}