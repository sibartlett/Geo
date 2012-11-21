using System;
using System.Collections.Generic;

namespace Geo.Geometries
{
    public class Triangle : Polygon
    {
        public new static readonly Triangle Empty = new Triangle();

        public Triangle()
        {
        }

        public Triangle(Coordinate p0, Coordinate p1, Coordinate p2)
            : base(new LinearRing(p0, p1, p2, p0))
        {
        }

        public Triangle(LinearRing shell, params LinearRing[] holes)
            : this(shell, (IEnumerable<LinearRing>)holes)
        {
        }

        public Triangle(LinearRing shell, IEnumerable<LinearRing> holes)
            : base(shell, holes)
        {
            if (!shell.IsClosed && shell.Coordinates.Count != 4)
                throw new ArgumentException("The Coordinate Sequence is not valid for a triangle.");
        }
    }
}
