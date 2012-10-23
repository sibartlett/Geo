using System;
using System.Collections.Generic;
using System.Text;
using Geo.Interfaces;

namespace Geo.Geometries
{
    public class GeometryCollection : IGeometry, IWktShape
    {
        public GeometryCollection()
        {
            Geometries = new List<IGeometry>();
        }

        public GeometryCollection(IEnumerable<IGeometry> geometries)
        {
            Geometries = new List<IGeometry>(geometries);
        }
        
        public List<IGeometry> Geometries { get; set; }

        public Envelope GetBounds()
        {
            Envelope envelope = null;
            foreach (var geometry in Geometries)
            {
                if (envelope == null)
                    envelope = geometry.GetBounds();
                else
                    envelope.Combine(geometry.GetBounds());
            }
            return envelope;
        }

        public string ToWktString()
        {
            var buf = new StringBuilder();
            buf.Append("GEOMETRYCOLLECTION ");
            if (Geometries.Count==0)
                buf.Append("EMPTY");
            else
            {
                buf.Append("(");
                for (int index = 0; index < Geometries.Count; index++)
                {
                    var geometry = Geometries[index];
                    var wktgeo = geometry as IWktShape;
                    if (wktgeo == null)
                        throw new NotSupportedException("Geometry of type '" + geometry.GetType().Name + "' does not support WKT.");
                    if(index > 0)
                        buf.Append(",");
                    buf.Append(wktgeo.ToWktString());
                }
                buf.Append(")");
            }
            return buf.ToString();
        }

        string IRavenIndexable.GetIndexString()
        {
            return ToWktString();
        }
    }
}
