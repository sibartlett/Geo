namespace Geo.Geometries
{
    public interface IPoint : IGeometry, ILatLng, IWktShape
    {
        double? Elevation { get; }
    }
}