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
        public string Source => "source";

        /// <summary>
        /// The GPS route number. GPX declares it <c>xsd:nonNegativeInteger</c>, so a value
        /// that is not one is neither read from nor written to a GPX file.
        /// </summary>
        public string Number => "number";

        /// <summary>
        /// The type (classification) of the route. GPX 1.0 has no such element, so it is
        /// read from and written to GPX 1.1 only.
        /// </summary>
        public string Type => "type";
    }
}
