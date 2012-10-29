namespace Geo.Interfaces
{
    public interface IWktGeometry : IGeometry
    {
        string ToWktString();
    }
}
