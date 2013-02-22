using Geo.IO.Spatial4n;
using Geo.Measure;

namespace Geo.Raven.Json
{
    public class RavenIndexStringReader : Spatial4nReader
    {
        protected override double ConvertCircleRadius(double radius)
        {
            return new Distance(radius, DistanceUnit.Km).SiValue;
        }
    }
}