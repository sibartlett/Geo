namespace Geo.Geometries
{
    public interface ILatLngCoordinate : IGeometry, IWktShape, IWktPart
    {
        double Latitude { get; }
        double Longitude { get; }
    }
}
