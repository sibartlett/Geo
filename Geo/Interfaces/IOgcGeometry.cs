namespace Geo.Interfaces
{
    public interface IOgcGeometry : IGeometry
    {
        string ToWktString();
    }
}
