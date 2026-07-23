namespace Geo.Gps.Metadata;

public class RouteMetadata : Metadata<RouteMetadata.MetadataKeys>
{
    public RouteMetadata()
        : base(new MetadataKeys()) { }

    public class MetadataKeys
    {
        internal MetadataKeys() { }

        public string Name => "name";
        public string Description => "description";
        public string Comment => "comment";
    }
}
