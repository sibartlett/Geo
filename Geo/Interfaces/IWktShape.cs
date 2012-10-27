namespace Geo.Interfaces
{
    public interface IWktShape : IGeometry
    {
        string ToWktString();
    }
}
