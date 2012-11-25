using Geo.IO.Wkt;

namespace Geo.IO.Wkb
{
    public class WkbWriterSettings
    {
        public WkbWriterSettings()
        {
            Encoding = WkbEncoding.LittleEndian;
            Triangle = false;
        }

        public WkbEncoding Encoding { get; set; }
        public bool Triangle { get; set; }

        public static WktWriterSettings NtsCompatible
        {
            get { return new WktWriterSettings(); }
        }
    }
}
