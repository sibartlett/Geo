using Geo.Interfaces;
using Geo.Reference;

namespace Geo
{
    public class GeoContext
    {
        private static GeoContext _current;

        public static GeoContext Current
        {
            get { return _current ?? (_current = new GeoContext()); }
            set { _current = value; }
        }

        public GeoContext()
        {
            GeodeticCalculator = SpheroidCalculator.Wgs84();
            LongitudeWrapping = false;
        }

        public IGeodeticCalculator GeodeticCalculator { get; set; }
        public bool LongitudeWrapping { get; set; }
    }
}
