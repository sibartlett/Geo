using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Linq;
using Geo.Measure;

namespace Geo.Geometries
{
    public class MultiPolygon : GeometryCollection, IMultiSurface
    {
        public new static readonly MultiPolygon Empty = new MultiPolygon();

        public MultiPolygon()
        {
        }

        public MultiPolygon(IEnumerable<Polygon> polygons)
            : base(polygons.Cast<IGeometry>())
        {
        }

        public MultiPolygon(params Polygon[] polygons)
            : base(polygons.Cast<IGeometry>())
        {
        }

        public Area GetArea()
        {
            return Geometries.Cast<Polygon>().Sum(x => x.GetArea());
        }
    }
}