using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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

    public class LineString<T> : List<T> where T : IPoint
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
                return IsEmpty ? default(T) : this[0];
            }
        }

        public T EndPoint
        {
            get
            {
                return IsEmpty ? default(T) : this[Count - 1];
            }
        }

        public Distance CalculateLength()
        {
            var distance = new Distance(0);

            if (Count > 1)
            {

                for (var i = 1; i < Count; i++)
                {
                    var line = this[i - 1].CalculateShortestLine(this[i]);
                    if(line !=null)
                        distance += line.Distance;
                }
            }

            return distance;
        }

        public bool IsEmpty
        {
            get { return Count == 0; }
        }

        public Bounds GetBounds()
        {
            return IsEmpty ? null :
                new Bounds(this.Min(x => x.Latitude), this.Min(x => x.Longitude), this.Max(x => x.Latitude), this.Max(x => x.Longitude));
        }
    }
}
