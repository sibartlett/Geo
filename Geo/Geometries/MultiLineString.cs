using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Linq;
using Geo.Measure;

namespace Geo.Geometries
{
    public class MultiLineString : GeometryCollection, IMultiCurve
    {
        public new static readonly MultiLineString Empty = new MultiLineString();

        public MultiLineString()
        {
        }

        public MultiLineString(IEnumerable<LineString> lineStrings) 
            : base(lineStrings.Cast<IGeometry>())
        {
        }

        public MultiLineString(params LineString[] lineStrings)
            : base(lineStrings.Cast<IGeometry>())
        {
        }

        public Distance GetLength()
        {
            return Geometries.Cast<LineString>().Sum(x => x.GetLength());
        }
    }
}