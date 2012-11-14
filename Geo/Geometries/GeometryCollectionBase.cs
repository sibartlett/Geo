using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public abstract class GeometryCollectionBase<TCollection, TElement> : IGeometry, IWktGeometry, IEquatable<TCollection>
        where TCollection : GeometryCollectionBase<TCollection, TElement>
        where TElement : class, IGeometry
    {
        protected GeometryCollectionBase(IEnumerable<TElement> lineStrings)
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

        public abstract string ToWktString();

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }

        protected string BuildWktString<TWktInterface>(string wktType, Func<TWktInterface, string> func)
        {
            var buf = new StringBuilder();
            buf.Append(wktType);
            buf.Append(" ");
            if (Geometries.Count == 0)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                for (int index = 0; index < Geometries.Count; index++)
                {
                    var geometry = Geometries[index];

                    if (geometry is TWktInterface)
                    {
                        if (index > 0)
                            buf.Append(",");
                        buf.Append(func((TWktInterface) (object) geometry));
                    }
                    else
                    {
                        throw new NotSupportedException("Geometry of type '" + geometry.GetType().Name + "' does not support WKT.");
                    }
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        #region Equality methods

        public bool Equals(TCollection other)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (Geometries.Count != other.Geometries.Count)
                return false;

            return !Geometries
                .Where((t, i) => !t.Equals(other.Geometries[i]))
                .Any();
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((TCollection) obj);
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