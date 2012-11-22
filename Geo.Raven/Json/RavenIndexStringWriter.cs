using Geo.IO.Spatial4n;
using Geo.Measure;

namespace Geo.Raven.Json
{
    public class RavenIndexStringWriter : Spatial4nWriter
    {
        protected override double ConvertCircleRadius(double radius)
        {
            return radius.ConvertTo(DistanceUnit.Km);
        }
    }
}