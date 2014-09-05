using Geo.Abstractions.Interfaces;
using Geo.IO.GeoJson;
using Geo.IO.Spatial4n;
using Geo.IO.Wkb;
using Geo.IO.Wkt;

namespace Geo.Abstractions
{
    public abstract class Geometry : SpatialObject, IGeometry
    {
        public abstract Envelope GetBounds();
        public abstract bool IsEmpty { get; }
        public abstract bool Is3D { get; }
        public abstract bool IsMeasured { get; }

        public string ToWktString()
        {
            return new WktWriter().Write(this);
        }

        public string ToWktString(WktWriterSettings settings)
        {
            return new WktWriter(settings).Write(this);
        }

        public byte[] ToWkbBinary()
        {
            return new WkbWriter().Write(this);
        }

        public byte[] ToWkbBinary(WkbWriterSettings settings)
        {
            return new WkbWriter(settings).Write(this);
        }

        string ISpatial4nShape.ToSpatial4nString()
        {
            return new Spatial4nWriter().Write(this);
        }

        ISpatial4nShape IRavenIndexable.GetSpatial4nShape()
        {
            return this;
		}

		public string ToGeoJson()
		{
			return GeoJson.Serialize(this);
		}
    }
}
