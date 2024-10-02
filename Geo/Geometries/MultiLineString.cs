using System.Collections.Generic;
using System.Linq;
using Geo.Abstractions.Interfaces;
using Geo.Linq;
using Geo.Measure;

namespace Geo.Geometries;

public class MultiLineString : GeometryCollection, IMultiCurve
{
    public static new readonly MultiLineString Empty = new();

    public MultiLineString() { }

    public MultiLineString(IEnumerable<LineString> lineStrings)
        : base(lineStrings) { }

    public MultiLineString(params LineString[] lineStrings)
        : base(lineStrings.Cast<IGeometry>()) { }

    public Distance GetLength()
    {
        return Geometries.Cast<LineString>().Sum(x => x.GetLength());
    }
}
