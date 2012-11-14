using System.Collections.Generic;
using Geo.Interfaces;
using Geo.Json;

namespace Geo.Geometries
{
    public class GeometryCollection : GeometryCollectionBase<GeometryCollection, IGeometry>, IGeoJsonGeometry
    {
        public static readonly GeometryCollection Empty = new GeometryCollection();

        public GeometryCollection(IEnumerable<IGeometry> geometries) : base(geometries)
        {
        }

        public GeometryCollection(params IGeometry[] geometries) : base(geometries)
        {
        }

        public override string ToWktString()
        {
            return BuildWktString<IWktGeometry>("GEOMETRYCOLLECTION", geometry => geometry.ToWktString());
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

        public static bool operator ==(GeometryCollection left, GeometryCollection right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(GeometryCollection left, GeometryCollection right)
        {
            return !(left == right);
        }

        #endregion
    }
}
