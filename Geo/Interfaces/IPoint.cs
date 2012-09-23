using Geo.Geometries;

namespace Geo.Interfaces
{
    public interface IPoint : IGeometry, ILatLng, IWktShape
    {
        double? Elevation { get; }
        Coordinate GetCoordinate();
    }
}