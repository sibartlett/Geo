namespace Geo.Gps.Metadata;

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

        public string Name => "name";
        public string Description => "description";
        public string Comment => "comment";
    }
}