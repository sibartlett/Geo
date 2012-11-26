using Geo.IO.Wkt;

namespace Geo.IO.Wkb
{
    public class WkbWriterSettings
    {
        public WkbWriterSettings()
        {
            Encoding = WkbEncoding.LittleEndian;
            Triangle = false;
            MaxDimesions = 4;
        }

        public int MaxDimesions { get; set; }
        public WkbEncoding Encoding { get; set; }
        public bool Triangle { get; set; }

        public static WktWriterSettings NtsCompatible
        {
            get { return new WktWriterSettings(); }
        }
    }
}
