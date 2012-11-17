using System.Collections.Generic;

namespace Geo.Geometries
{
    public class MultiPoint : GeometryCollectionBase<MultiPoint, Point>
    {
        public static readonly MultiPoint Empty = new MultiPoint();

        public MultiPoint() : base(new Point[0])
        {
        }

        public MultiPoint(IEnumerable<Point> points) : base(points)
        {
        }

        public MultiPoint(params Point[] points) : base(points)
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