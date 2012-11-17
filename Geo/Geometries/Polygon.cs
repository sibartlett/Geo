using System.Collections.Generic;
using System.Linq;
using Geo.IO.GeoJson;
using Geo.IO.Wkt;
using Geo.Interfaces;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : SpatialObject<Polygon>, IGeometry, IOgcGeometry, IGeoJsonGeometry
    {
        public static readonly Polygon Empty = new Polygon(new LinearRing());

        public Polygon() : this(null)
        {
        }

        public Polygon(LinearRing shell, IEnumerable<LinearRing> holes)
        {
            Shell = shell;
            Holes = new GeometrySequence<LinearRing>(holes ?? new LinearRing[0]);
        }

        public Polygon(LinearRing shell, params LinearRing[] holes) : this(shell, (IEnumerable<LinearRing>) holes)
        {
        }

        public LinearRing Shell { get; private set; }
        public GeometrySequence<LinearRing> Holes { get; private set; }

        public bool IsEmpty
        {
            get { return Shell.IsEmpty; }
        }

        public bool HasElevation
        {
            get { return Shell.HasElevation; }
        }

        public bool HasM
        {
            get { return Shell.HasM; }
        }

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

        public override bool Equals(Polygon other, SpatialEqualityOptions options)
        {
            if (ReferenceEquals(null, other))
                return false;

            if (IsEmpty && other.IsEmpty)
                return true;

            return Shell.Equals(other.Shell, options) && Equals(Holes, other.Holes, options);
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
            unchecked
            {
                return ((Shell != null ? Shell.GetHashCode(options) : 0)*397) ^ (Holes != null ? Holes.GetHashCode(options) : 0);
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
