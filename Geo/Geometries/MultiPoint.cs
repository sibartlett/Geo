using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;

namespace Geo.Geometries;

public class MultiPoint : GeometryCollection
{
    public new static readonly MultiPoint Empty = new();

    public MultiPoint()
    {
    }

    public MultiPoint(IEnumerable<Point> points)
        : base(points)
    {
    }

    public MultiPoint(params Point[] points)
        : base(points.Cast<IGeometry>())
    {
    }
}