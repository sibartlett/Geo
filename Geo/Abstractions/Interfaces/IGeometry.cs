using Geo.IO.Wkb;
using Geo.IO.Wkt;

namespace Geo.Abstractions.Interfaces
{
    public interface IGeometry : ISpatialEquatable, IGeoJsonObject
    {
        Envelope GetBounds();

        bool IsEmpty { get; }
        bool Is3D { get; }
        bool IsMeasured { get; }

		string ToWktString();
		string ToWktString(WktWriterSettings settings);
		byte[] ToWkbBinary();
		byte[] ToWkbBinary(WkbWriterSettings settings);
    }

    public interface ICurve : IGeometry, IHasLength
    {
        bool IsClosed { get; }
    }

    public interface ISurface : IGeometry, IHasArea
    {
    }

    public interface IMultiCurve : IGeometry, IHasLength
    {
    }

    public interface IMultiSurface : IGeometry, IHasArea
    {
    }
}
