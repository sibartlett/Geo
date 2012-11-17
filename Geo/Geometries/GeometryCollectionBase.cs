using System.Collections.Generic;
using System.Linq;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public abstract class GeometryCollectionBase<TCollection, TElement>
        : SpatialObject<TCollection>, IGeometry, IOgcGeometry, IGeoJsonGeometry
        where TCollection : GeometryCollectionBase<TCollection, TElement>
        where TElement : class, IGeometry
    {
        internal GeometryCollectionBase(IEnumerable<TElement> lineStrings)
        {
            Geometries = new List<TElement>(lineStrings ?? new TElement[0]);
        }
        
        public List<TElement> Geometries { get; set; }

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

        public Area GetArea()
        {
            return new Area(Geometries.Sum(geometry => geometry.GetArea().SiValue));
        }

        public Distance GetLength()
        {
            return new Distance(Geometries.Sum(geometry => geometry.GetLength().SiValue));
        }

        public bool IsEmpty
        {
            get { return Geometries.Count == 0; }
        }

        public bool HasElevation
        {
            get { return Geometries.Any(x => x.HasElevation); }
        }

        public bool HasM
        {
            get { return Geometries.Any(x => x.HasM); }
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

        public string ToWktString()
        {
            return new WktWriter().Write(this);
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }

        #region Equality methods

        public override bool Equals(TCollection other, SpatialEqualityOptions options)
        {
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