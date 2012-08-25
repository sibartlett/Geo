namespace Geo.Gps.Metadata
{
    public class TrackMetadata : Metadata<TrackMetadata.MetadataKeys>
    {
        public TrackMetadata() : base(new MetadataKeys())
        {
        }

        public class MetadataKeys
        {
            internal MetadataKeys()
            {
            }

            public string Name { get { return "name"; } }
            public string Description { get { return "description"; } }
            public string Comment { get { return "comment"; } }
        }
    }
}