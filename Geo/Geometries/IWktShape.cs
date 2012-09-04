namespace Geo.Geometries
{
    public interface IWktShape
    {
        string ToWktString();
    }

    public interface IWktPart
    {
        string ToWktPartString();
    }
}
