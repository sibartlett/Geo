using Geo.Geometries;

namespace Geo.Interfaces
{
    public interface IPoint : IGeometry, ILatLng, IWktGeometry
    {
        double? Elevation { get; }
        Coordinate GetCoordinate();
    }
}