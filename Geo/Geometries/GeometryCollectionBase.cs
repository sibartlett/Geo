using System;
using System.Collections.Generic;
using System.Linq;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public abstract class GeometryCollectionBase<TCollection, TElement> : IGeometry, IWktGeometry, IGeoJsonGeometry, ISpatialEquatable<TCollection>
        where TCollection : GeometryCollectionBase<TCollection, TElement>
        where TElement : class, IGeometry, ISpatialEquatable
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

        public bool IsEmpty { get { return Geometries.Count == 0; } }
        public bool HasElevation { get { return Geometries.Any(x => x.HasElevation); } }
        public bool HasM { get { return Geometries.Any(x => x.HasM); } }

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

        public bool Equals(TCollection other, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (Geometries.Count != other.Geometries.Count)
                return false;

            return !Geometries
                .Where((t, i) => !t.Equals(other.Geometries[i], options))
                .Any();
        }

        public bool Equals(TCollection other)
        {
            return Equals(other, GeoContext.Current.EqualityOptions);
        }

        public bool Equals(object obj, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TCollection) obj);
        }

        public override bool Equals(object obj)
        {
            return Equals(obj, GeoContext.Current.EqualityOptions);
        }

        public override int GetHashCode()
        {
            return Geometries
                .Select(x => x.GetHashCode())
                .Aggregate(0, (current, result) => (current * 397) ^ result);
        }

        #endregion
    }
}