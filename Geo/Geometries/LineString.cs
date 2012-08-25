using System.Collections.Generic;
using Geo.Measure;

namespace Geo.Geometries
{
    public class LineString : LineString<Point>
    {
        public LineString()
        {
        }

        public LineString(IEnumerable<Point> items) : base(items)
        {
        }
    }

    public class LineString<T> : MultiPoint<T> where T : IPoint
    {
        public LineString()
        {
        }

        public LineString(IEnumerable<T> items) : base(items)
        {
        }

        public T StartPoint
        {
            get
            {
                return IsEmpty ? default(T) : InternalList[0];
            }
        }

        public T EndPoint
        {
            get
            {
                return IsEmpty ? default(T) : InternalList[InternalList.Count - 1];
            }
        }

        public Distance CalculateLength()
        {
            var distance = new Distance(0);

            if (InternalList.Count > 1)
                for (var i = 1; i < Count; i++)
                    distance += InternalList[i - 1].CalculateShortestLine(InternalList[i]).Distance;

            return distance;
        }
    }
}
