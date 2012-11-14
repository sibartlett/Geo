using System;
using System.Collections.Generic;
using System.Linq;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Json;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : IGeometry, IWktGeometry, IGeoJsonGeometry, IEquatable<Polygon>
    {
        public static readonly Polygon Empty = new Polygon(new LinearRing());

        public Polygon(LinearRing shell, IEnumerable<LinearRing> holes)
        {
            Shell = shell;
            Holes = new GeometrySequence<LinearRing>(holes ?? new LinearRing[0]);
        }

        public Polygon(LinearRing shell, params LinearRing[] holes)
        {
            Shell = shell;
            Holes = new GeometrySequence<LinearRing>(holes ?? new LinearRing[0]);
        }

        public LinearRing Shell { get; private set; }
        public GeometrySequence<LinearRing> Holes { get; private set; }

        public bool IsEmpty { get { return Shell.IsEmpty; } }
        public bool HasElevation { get { return Shell.HasElevation; } }
        public bool HasM { get { return Shell.HasM; } }

        public Distance GetLength()
        {
            return Shell.GetLength();
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }

        public string ToWktString()
        {
            return new WktWriter().Write(this);
        }

        public Envelope GetBounds()
        {
            return Shell.GetBounds();
        }

        public Area GetArea()
        {
            return Shell.GetArea() -  new Area(Holes.Sum(x => x.GetArea().SiValue));
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

        #region Equality methods

        public bool Equals(Polygon other)
        {
            return !ReferenceEquals(null, other) && Equals(Shell, other.Shell) && Equals(Holes, other.Holes);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((Polygon) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Shell != null ? Shell.GetHashCode() : 0)*397) ^ (Holes != null ? Holes.GetHashCode() : 0);
            }
        }

        public static bool operator ==(Polygon left, Polygon right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(Polygon left, Polygon right)
        {
            return !(left == right);
        }

        #endregion
    }
}
