namespace Geo.Geometries
{
    public interface IGeometry : IRavenIndexable
    {
        Envelope GetBounds();
    }
}
