namespace Geo.Geometries
{
    public interface IPoint : ILatLngCoordinate
    {
        double? Elevation { get; }
    }
}