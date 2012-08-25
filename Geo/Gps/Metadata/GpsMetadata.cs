namespace Geo.Gps.Metadata
{
    public class GpsMetadata : Metadata<GpsMetadata.MetadataKeys>
    {
        public GpsMetadata() : base(new MetadataKeys())
        {
        }

        public class MetadataKeys
        {
            public AuthorKeys Author = new AuthorKeys();
            public CopyrightKeys Copyright = new CopyrightKeys();
            public VehicleKeys Vehicle = new VehicleKeys();

            internal MetadataKeys()
            {
            }

            public string Name { get { return "name"; } }
            public string Description { get { return "description"; } }
            public string Keywords { get { return "keywords"; } }
            public string Link { get { return "link"; } }
            public string Software { get { return "creator"; } }
            
            public class AuthorKeys
            {
                public string Name { get { return "author.name"; } }
                public string Email { get { return "author.email"; } }
                public string Link { get { return "author.link"; } }
            }
            
            public class CopyrightKeys
            {
                public string Author { get { return "copyright.author"; } }
                public string Year { get { return "copyright.year"; } }
                public string License { get { return "copyright.license"; } }
            }
            
            public class VehicleKeys
            {
                public string Model { get { return "vehicle.model"; } }
                public string Identifier { get { return "vehicle.identifier"; } }
                public string Crew1 { get { return "vehicle.crew1"; } }
                public string Crew2 { get { return "vehicle.crew2"; } }
            }
        }
    }
}