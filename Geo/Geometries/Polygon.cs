using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions;
using Geo.Abstractions.Interfaces;
using Geo.IO.GeoJson;
using Geo.IO.Spatial4n;
using Geo.IO.Wkb;
using Geo.IO.Wkt;
using Geo.Measure;

namespace Geo.Geometries
{
    public class Polygon : SpatialObject, ISurface, IOgcGeometry, IGeoJsonGeometry
    {
        public static readonly Polygon Empty = new Polygon();

        public Polygon() : this(null)
        {
        }

        public Polygon(LinearRing shell, IEnumerable<LinearRing> holes)
        {
            Shell = shell;
            Holes = new SpatialReadOnlyCollection<LinearRing>(holes ?? new LinearRing[0]);
        }

        public Polygon(LinearRing shell, params LinearRing[] holes) : this(shell, (IEnumerable<LinearRing>) holes)
        {
        }

        public LinearRing Shell { get; private set; }
        public SpatialReadOnlyCollection<LinearRing> Holes { get; private set; }

        public bool IsEmpty
        {
            get { return Shell == null || Shell.IsEmpty; }
        }

        public bool HasElevation
        {
            get { return !IsEmpty && Shell.HasElevation; }
        }

        public bool HasM
        {
            get { return !IsEmpty && Shell.HasM; }
        }

        string ISpatial4nShape.ToSpatial4nString()
        {
            return new Spatial4nWriter().Write(this);
        }

        ISpatial4nShape IRavenIndexable.GetSpatial4nShape()
        {
            return this;
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

        public Envelope GetBounds()
        {
            return Shell.GetBounds();
        }

        public Area GetArea()
        {
            var calculator = GeoContext.Current.GeodeticCalculator;
            var area = calculator.CalculateArea(Shell.Coordinates);
            return Holes.Aggregate(area, (current, hole) => current - calculator.CalculateArea(hole.Coordinates));
        }

        public string ToGeoJson()
        {
            return GeoJson.Serialize(this);
        }

        #region Equality methods

        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override bool Equals(object obj, SpatialEqualityOptions options)
        {
            var other = obj as Polygon;

            if (ReferenceEquals(null, other))
                return false;

            if (IsEmpty && other.IsEmpty)
                return true;

            return Shell.Equals(other.Shell, options)
                && Holes.Count == other.Holes.Count
                && !Holes
                .Where((t, i) => !t.Equals(other.Holes[i], options))
                .Any();
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
