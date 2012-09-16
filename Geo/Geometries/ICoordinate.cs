namespace Geo.Geometries
{
    public interface ICoordinate : ILatLng, IWktPart
    {
        double? Elevation { get; }
    }
}
