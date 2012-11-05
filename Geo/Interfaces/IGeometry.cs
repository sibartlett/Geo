using Geo.Geometries;

namespace Geo.Interfaces
{
    public interface IGeometry : IRavenIndexable, IHasArea, IHasLength
    {
        Envelope GetBounds();

        bool IsEmpty { get; }
        bool HasElevation { get; }
        bool HasM { get; }
    }
}
