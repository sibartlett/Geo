using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Geo.Geometries
{
    public class MultiPoint : MultiPoint<Point>
    {
        public MultiPoint()
        {
        }

        public MultiPoint(IEnumerable<Point> items) : base(items)
        {
        }
    }

    public class MultiPoint<T> : IEnumerable<T> where T : IPoint
    {
        protected List<T> InternalList = new List<T>();

        public MultiPoint()
        {
        }

        public MultiPoint(IEnumerable<T> items)
        {
            InternalList.AddRange(items);
        }

        public IEnumerator<T> GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return InternalList.GetEnumerator();
        }

        public bool IsEmpty
        {
            get { return InternalList.Count == 0; }
        }

        public int Count
        {
            get { return InternalList.Count; }
        }

        public void Add(T point)
        {
            InternalList.Add(point);
        }

        public T this[int index]
        {
            get { return InternalList[index]; }
        }

        public Bounds GetBounds()
        {
            return IsEmpty ? null :
                new Bounds(this.Min(x => x.Latitude), this.Min(x => x.Longitude), this.Max(x => x.Latitude), this.Max(x => x.Longitude));
        }
    }
}