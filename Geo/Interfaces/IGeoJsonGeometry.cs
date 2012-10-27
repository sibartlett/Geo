namespace Geo.Interfaces
{
    public interface IGeoJsonGeometry : IGeoJsonObject, IGeometry
    {
        object ToGeoJsonObject();
    }
}
