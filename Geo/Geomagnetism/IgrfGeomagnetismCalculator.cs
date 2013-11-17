using Geo.Geodesy;
using Geo.Geomagnetism.Models;

namespace Geo.Geomagnetism
{
    public class IgrfGeomagnetismCalculator : GeomagnetismCalculator
    {
        public IgrfGeomagnetismCalculator() : base(IgrfModelFactory.GetModels())
        {
        }

        public IgrfGeomagnetismCalculator(Spheroid spheroid) : base(spheroid, IgrfModelFactory.GetModels())
        {
        }
    }
}