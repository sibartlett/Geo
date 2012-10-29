using Geo.Geometries;
using Geo.Measure;

namespace Geo.Interfaces
{
    public interface IGeometry : IRavenIndexable
    {
        Envelope GetBounds();
        Area GetArea();
    }
}
