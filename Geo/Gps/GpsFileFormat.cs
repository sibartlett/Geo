using System;

namespace Geo.Gps
{
    public class GpsFileFormat
    {
        public GpsFileFormat(string extension, string name)
        {
            Extension = extension;
            Name = name;
        }

        public GpsFileFormat(string extension, string name, string specificationUrl)
        {
            Extension = extension;
            Name = name;
            SpecificationUri = new Uri(specificationUrl);
        }

        public string Extension { get; private set; }
        public string Name { get; private set; }
        public Uri SpecificationUri { get; private set; }
    }
}
