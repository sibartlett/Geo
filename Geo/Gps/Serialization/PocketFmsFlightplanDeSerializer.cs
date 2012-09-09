using System;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;
using Geo.Gps.Serialization.Xml.PocketFms;

namespace Geo.Gps.Serialization
{
    public class PocketFmsFlightplanDeSerializer : GpsXmlDeSerializer<PocketFmsFlightplan>
    {
        public override GpsFeatures SupportedFeatures
        {
            get { return GpsFeatures.Routes; }
        }

        public override string[] FileExtensions
        {
            get { return new[] { "xml"}; }
        }

        public override Uri FileFormatSpecificationUri
        {
            get { return new Uri("http://www.PocketFMS.com/XMLSchema/PocketFMSNavlog-1.2.0.xsd"); }
        }

        protected override GpsData DeSerialize(PocketFmsFlightplan xml)
        {
            var route = new Route();
            route.LineString.Points.Add(new Point((double)xml.LIB[0].FromPoint.Latitude, (double)xml.LIB[0].FromPoint.Longitude));
            foreach (var lib in xml.LIB)
            {
                route.LineString.Points.Add(new Point((double)lib.ToPoint.Latitude, (double)lib.ToPoint.Longitude));
            }

            var data = new GpsData();
            data.Routes.Add(route);

            data.Metadata.Attribute(x => x.Vehicle.Crew1, xml.META.PilotInCommand);
            data.Metadata.Attribute(x => x.Vehicle.Identifier, xml.META.AircraftIdentification);
            data.Metadata.Attribute(x => x.Vehicle.Model, xml.META.AircraftType);

            return data;
        }
    }
}
