using Geo.Geometries;

namespace Geo.Interfaces
{
    public interface IGeometry : IRavenIndexable
    {
        Envelope GetBounds();
    }
}
