using System.Collections.Generic;
using System.Linq;
using Geo.Interfaces;
using Geo.Json;

namespace Geo.Geometries
{
    public class MultiPoint : GeometryCollectionBase<Point>, IGeoJsonGeometry
    {
        public MultiPoint()
        {
        }

        public MultiPoint(IEnumerable<Point> points) : base(points)
        {
        }

        public MultiPoint(params Point[] points) : base(points)
        {
        }

        public override string ToWktString()
        {
            return BuildWktString<IWktPart>("MULTIPOINT", geometry => geometry.ToWktPartString());
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

        public static bool operator ==(MultiPoint left, MultiPoint right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(MultiPoint left, MultiPoint right)
        {
            return !(left == right);
        }

        #endregion
    }
}