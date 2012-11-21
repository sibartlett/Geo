namespace Geo.IO.Wkt
{
    public class WktWriterSettings
    {
        public WktWriterSettings()
        {
            LinearRing = false;
            Triangle = false;
        }

        public bool LinearRing { get; set; }
        public bool Triangle { get; set; }

        public static WktWriterSettings NtsCompatible
        {
            get { return new WktWriterSettings { LinearRing = true }; }
        }
    }
}
