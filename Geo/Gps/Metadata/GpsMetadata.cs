using System;

namespace Geo.Gps.Metadata;

public class GpsMetadata : Metadata<GpsMetadata.MetadataKeys>
{
    public GpsMetadata()
        : base(new MetadataKeys()) { }

    /// <summary>
    /// When the file was created, or <c>null</c> when it does not say.
    /// </summary>
    /// <remarks>
    /// A property rather than one of the keyed attributes, because the attributes are
    /// strings and this is a date: GPX declares the element <c>xsd:dateTime</c>, so a
    /// value kept as text could be written back in a form no other reader would accept.
    /// <see cref="Waypoint.TimeUtc" /> already carries a GPX time this way.
    /// <para>
    /// GPX 1.1 holds this in &lt;metadata&gt;, 1.0 as a direct child of &lt;gpx&gt;.
    /// Only the GPX deserializers set it; a file read from a format with no such field
    /// leaves it null.
    /// </para>
    /// </remarks>
    public DateTime? TimeUtc { get; set; }

    public class MetadataKeys
    {
        public AuthorKeys Author = new();
        public CopyrightKeys Copyright = new();
        public VehicleKeys Vehicle = new();

        internal MetadataKeys() { }

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
