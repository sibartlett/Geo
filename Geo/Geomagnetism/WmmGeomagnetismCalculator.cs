using Geo.Geodesy;
using Geo.Geomagnetism.Models;

namespace Geo.Geomagnetism
{
    public class WmmGeomagnetismCalculator : GeomagnetismCalculator
    {
        private static readonly IGeomagneticModel[] GeomagneticModels = new IGeomagneticModel[]
            {
                new Wmm1985(),
                new Wmm1990(),
                new Wmm1995(),
                new Wmm2000(),
                new Wmm2005(),
                new Wmm2010()
            };

        public WmmGeomagnetismCalculator() : base(GeomagneticModels)
        {
        }

        public WmmGeomagnetismCalculator(Spheroid spheroid) : base(spheroid, GeomagneticModels)
        {
        }
    }
}