namespace Geo.Gps.Metadata;

public class GpsMetadata : Metadata<GpsMetadata.MetadataKeys>
{
    public GpsMetadata() : base(new MetadataKeys())
    {
    }

    public class MetadataKeys
    {
        public AuthorKeys Author = new();
        public CopyrightKeys Copyright = new();
        public VehicleKeys Vehicle = new();

        internal MetadataKeys()
        {
        }

        public string Name => "name";
        public string Description => "description";
        public string Keywords => "keywords";
        public string Link => "link";
        public string Software => "creator";

        public class AuthorKeys
        {
            public string Name => "author.name";
            public string Email => "author.email";
            public string Link => "author.link";
        }

        public class CopyrightKeys
        {
            public string Author => "copyright.author";
            public string Year => "copyright.year";
            public string License => "copyright.license";
        }

        public class VehicleKeys
        {
            public string Model => "vehicle.model";
            public string Identifier => "vehicle.identifier";
            public string Crew1 => "vehicle.crew1";
            public string Crew2 => "vehicle.crew2";
        }
    }
}