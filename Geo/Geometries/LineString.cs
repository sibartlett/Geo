using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    public class LineString<T> : IEnumerable<T> where T : IPoint
    {
        private readonly List<T> _internalList;

        public LineString()
        {
            _internalList = new List<T>();
        }

        public LineString(IEnumerable<T> items)
        {
            _internalList = new List<T>(items);
        }

        public T StartPoint
        {
            get
            {
                return IsEmpty ? default(T) : _internalList[0];
            }
        }

        public T EndPoint
        {
            get
            {
                return IsEmpty ? default(T) : _internalList[_internalList.Count - 1];
            }
        }

        public Distance CalculateLength()
        {
            var distance = new Distance(0);

            if (_internalList.Count > 1)
                for (var i = 1; i < Count; i++)
                    distance += _internalList[i - 1].CalculateShortestLine(_internalList[i]).Distance;

            return distance;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _internalList.GetEnumerator();
        }

        public bool IsEmpty
        {
            get { return _internalList.Count == 0; }
        }

        public int Count
        {
            get { return _internalList.Count; }
        }

        public void Add(T point)
        {
            _internalList.Add(point);
        }

        public T this[int index]
        {
            get { return _internalList[index]; }
        }

        public Bounds GetBounds()
        {
            return IsEmpty ? null :
                new Bounds(this.Min(x => x.Latitude), this.Min(x => x.Longitude), this.Max(x => x.Latitude), this.Max(x => x.Longitude));
        }
    }
}
