using Geo.Abstractions.Interfaces;
using Geo.Geodesy;

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
            EqualityOptions = new SpatialEqualityOptions();
            LongitudeWrapping = false;
        }

        public IGeodeticCalculator GeodeticCalculator { get; set; }
        public SpatialEqualityOptions EqualityOptions { get; set; }
        public bool LongitudeWrapping { get; set; }
    }
}
