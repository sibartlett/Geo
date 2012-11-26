using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.IO.GeoJson;
using Geo.IO.Spatial4n;
using Geo.IO.Wkb;
using Geo.IO.Wkt;

namespace Geo.Geometries
{
    public class GeometryCollection : SpatialObject, IGeometry, IOgcGeometry, IGeoJsonGeometry
    {
        public static readonly GeometryCollection Empty = new GeometryCollection();

        public GeometryCollection()
        {
            Geometries = new SpatialReadOnlyCollection<IGeometry>(new IGeometry[0]);
        }

        public GeometryCollection(IEnumerable<IGeometry> geometries)
        {
            var items = (geometries ?? new IGeometry[0]).ToList();
            Geometries = new SpatialReadOnlyCollection<IGeometry>(items);
        }

        public GeometryCollection(params IGeometry[] geometries)
            : this((IEnumerable<IGeometry>)geometries)
        {
        }

        public SpatialReadOnlyCollection<IGeometry> Geometries { get; private set; }

        public Envelope GetBounds()
        {
            Envelope envelope = null;
            foreach (var geometry in Geometries)
            {
                if (envelope == null)
                    envelope = geometry.GetBounds();
                else
                    envelope.Combine(geometry.GetBounds());
            }
            return envelope;
        }

        public bool IsEmpty
        {
            get { return Geometries.Count == 0; }
        }

        public bool Is3D
        {
            get { return Geometries.Any(x => x.Is3D); }
        }

        public bool IsMeasured
        {
            get { return Geometries.Any(x => x.IsMeasured); }
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

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

        #region Equality methods

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as GeometryCollection;

            if (ReferenceEquals(null, other))
                return false;

            if (Geometries.Count != other.Geometries.Count)
                return false;

            return !Geometries
                .Where((t, i) => !t.Equals(other.Geometries[i], options))
                .Any();
        }

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override int GetHashCode(SpatialEqualityOptions options)
        {
            return Geometries
                .Select(x => x.GetHashCode(options))
                .Aggregate(0, (current, result) => (current * 397) ^ result);
        }

        #endregion
    }
}
