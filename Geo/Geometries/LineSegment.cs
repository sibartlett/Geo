using System;

namespace Geo.Geometries
{
    public class LineSegment
    {
        public LineSegment(Coordinate coordinate1, Coordinate coordinate2)
        {
            if (coordinate1 == null)
                throw new ArgumentNullException("coordinate1");
            if (coordinate2 == null)
                throw new ArgumentNullException("coordinate2");

            Coordinate1 = coordinate1;
            Coordinate2 = coordinate2;
        }

        public Coordinate Coordinate1 { get; private set; }
        public Coordinate Coordinate2 { get; private set; }
        
        public Envelope GetBounds()
        {
            return Coordinate1.GetBounds().Combine(Coordinate2.GetBounds());
        }

        protected bool Equals(LineSegment other)
        {
            return Equals(Coordinate1, other.Coordinate1) && Equals(Coordinate2, other.Coordinate2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((LineSegment) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                return ((Coordinate1 != null ? Coordinate1.GetHashCode() : 0)*397) ^ (Coordinate2 != null ? Coordinate2.GetHashCode() : 0);
            }
        }

        public static bool operator ==(LineSegment left, LineSegment right)
        {
            if (ReferenceEquals(left, null) && ReferenceEquals(right, null))
                return true;
            return !ReferenceEquals(left, null) && !ReferenceEquals(right, null) && left.Equals(right);
        }

        public static bool operator !=(LineSegment left, LineSegment right)
        {
            return !(left == right);
        }
    }
}
