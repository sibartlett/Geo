using System.Collections.Generic;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class MultiPolygon : GeometryCollectionBase<MultiPolygon, Polygon>, IGeoJsonGeometry
    {
        public static readonly MultiPolygon Empty = new MultiPolygon();

        public MultiPolygon(IEnumerable<Polygon> polygons) : base(polygons)
        {
        }

        public MultiPolygon(params Polygon[] polygons) : base(polygons)
        {
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

        public static bool operator ==(MultiPolygon left, MultiPolygon right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(MultiPolygon left, MultiPolygon right)
        {
            return !(left == right);
        }

        #endregion
    }
}