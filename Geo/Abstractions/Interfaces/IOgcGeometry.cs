using Geo.IO.Wkb;
using Geo.IO.Wkt;

namespace Geo.Abstractions.Interfaces
{
    public interface IOgcGeometry : IGeometry
    {
        string ToWktString();
        string ToWktString(WktWriterSettings settings);
        byte[] ToWkbBinary();
        byte[] ToWkbBinary(WkbWriterSettings settings);
    }
}
