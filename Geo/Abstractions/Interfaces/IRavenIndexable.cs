namespace Geo.Abstractions.Interfaces
{
    public interface IRavenIndexable
    {
        ISpatial4nShape GetSpatial4nShape();
    }
}