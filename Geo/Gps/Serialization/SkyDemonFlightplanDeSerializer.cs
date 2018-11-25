using System.Globalization;
using System.Text.RegularExpressions;
using Geo.Geometries;
using Geo.Gps.Serialization.Xml;
using Geo.Gps.Serialization.Xml.SkyDemon;
using System.Xml;

namespace Geo.Gps.Serialization
{
    public class SkyDemonFlightplanDeSerializer : GpsXmlDeSerializer<SkyDemonFlightplan>
    {
        public override GpsFileFormat[] FileFormats
        {
            get
            {
                return new[]
                           {
                               new GpsFileFormat("flightplan", "SkyDemon Flightplan"),
                           };
            }
        }

        public override GpsFeatures SupportedFeatures
        {
            get { return GpsFeatures.Routes; }
        }

        private Route ConvertRoute(SkyDemonRoute route)
        {
            var result = new Route();
            result.Coordinates.Add(ParseCoordinate(route.Start));
            foreach (var rhumbLine in route.RhumbLineRoute)
            {
                result.Coordinates.Add(ParseCoordinate(rhumbLine.To));
            }
            return result;
        }

        private const string COORD_REGEX1 = @"^(?<dir>[NnSs])(?<d>\d\d)(?<m>\d\d)(?<s>\d\d.\d\d)$";
        private const string COORD_REGEX2 = @"^(?<dir>[EeWw])(?<d>\d\d\d)(?<m>\d\d)(?<s>\d\d.\d\d)$";

        private Coordinate ParseCoordinate(string c)
        {
            var ord = c.Trim().Split(' ');

            var match1 = Regex.Match(ord[0], COORD_REGEX1);
            var match2 = Regex.Match(ord[1], COORD_REGEX2);

            var lat = double.Parse(match1.Groups["d"].Value, CultureInfo.InvariantCulture) +
                      double.Parse(match1.Groups["m"].Value, CultureInfo.InvariantCulture) / 60 +
                      double.Parse(match1.Groups["s"].Value, CultureInfo.InvariantCulture) / 3600;

            var lon = double.Parse(match2.Groups["d"].Value, CultureInfo.InvariantCulture) +
                      double.Parse(match2.Groups["m"].Value, CultureInfo.InvariantCulture) / 60+
                      double.Parse(match2.Groups["s"].Value, CultureInfo.InvariantCulture) / 3600;

            var latd = Regex.IsMatch(match1.Groups["dir"].Value, "[NnEe]") ? 1 : -1;
            var lond = Regex.IsMatch(match2.Groups["dir"].Value, "[NnEe]") ? 1 : -1;

            return new Coordinate(lat * latd, lon * lond);
        }

        protected override bool CanDeSerialize(XmlReader xml) {
            return xml.Name == "DivelementsFlightPlanner";
        }

        protected override GpsData DeSerialize(SkyDemonFlightplan xml)
        {
            if (xml == null)
                return null;

            //N514807.00 W0000930.00
            var data = new GpsData();

            if(xml.PrimaryRoute != null)
            {
                data.Routes.Add(ConvertRoute(xml.PrimaryRoute));
            }

            if(xml.Routes != null)
            {
                foreach (var route in xml.Routes)
                {
                    data.Routes.Add(ConvertRoute(route));
                }
            }

            if(xml.Aircraft != null && xml.Aircraft.Length > 0)
            {
                data.Metadata.Attribute(x => x.Vehicle.Identifier, xml.Aircraft[0].Registration);
                data.Metadata.Attribute(x => x.Vehicle.Model, xml.Aircraft[0].Type);
            }

            return data;
        }
    }
}