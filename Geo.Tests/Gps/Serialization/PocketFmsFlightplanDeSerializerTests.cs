using System.IO;
using System.Linq;
using System.Text;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class PocketFmsFlightplanDeSerializerTests : SerializerTestFixtureBase
{
    private static StreamWrapper Wrap(string content)
    {
        var bytes = Encoding.UTF8.GetBytes(content);
        return new StreamWrapper(new MemoryStream(bytes));
    }

    private FileStream OpenReference()
    {
        var fileInfo = GetReferenceFileDirectory("pocketfms")
            .EnumerateFiles()
            .First(x => x.Name == "pocketfms_fp.xml");
        return new FileStream(fileInfo.FullName, FileMode.Open);
    }

    [Fact]
    public void CanParse()
    {
        using var stream = OpenReference();
        var file = new PocketFmsFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));
        Assert.NotNull(file);
    }

    [Fact]
    public void Parses_route_waypoints_and_metadata()
    {
        using var stream = OpenReference();
        var file = new PocketFmsFlightplanDeSerializer().DeSerialize(new StreamWrapper(stream));

        // Three legs -> the FromPoint of the first leg plus each ToPoint.
        Assert.Single(file.Routes);
        Assert.Equal(4, file.Routes[0].Waypoints.Count);
        Assert.Equal(51.158882, file.Routes[0].Waypoints[0].Coordinate.Latitude, 6);
        Assert.Equal(14.950277, file.Routes[0].Waypoints[0].Coordinate.Longitude, 6);

        Assert.Equal("TOBIAS KRETSCHMAR", file.Metadata.Attribute(x => x.Vehicle.Crew1));
    }

    [Fact]
    public void CanDeSerialize_returns_true_for_reference_file()
    {
        using var stream = OpenReference();
        Assert.True(
            new PocketFmsFlightplanDeSerializer().CanDeSerialize(new StreamWrapper(stream))
        );
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_other_xml()
    {
        Assert.False(new PocketFmsFlightplanDeSerializer().CanDeSerialize(Wrap("<foo />")));
    }

    [Fact]
    public void CanDeSerialize_returns_false_for_malformed_xml()
    {
        Assert.False(new PocketFmsFlightplanDeSerializer().CanDeSerialize(Wrap("<broken")));
    }

    [Fact]
    public void DeSerialize_returns_null_for_malformed_xml()
    {
        Assert.Null(new PocketFmsFlightplanDeSerializer().DeSerialize(Wrap("<broken")));
    }

    [Fact]
    public void DeSerialize_returns_null_for_a_flightplan_with_no_legs()
    {
        // A document with no <LIB> holds no route. It used to be indexed at [0]
        // regardless, so a file this deserializer had already claimed left it as an
        // IndexOutOfRangeException instead of the null it reports for one it cannot
        // read.
        Assert.Null(
            new PocketFmsFlightplanDeSerializer().DeSerialize(
                Wrap("<PocketFMSFlightplan><META /></PocketFMSFlightplan>")
            )
        );
    }

    [Fact]
    public void DeSerialize_returns_null_when_a_leg_has_no_coordinates()
    {
        Assert.Null(
            new PocketFmsFlightplanDeSerializer().DeSerialize(
                Wrap(
                    "<PocketFMSFlightplan><LIB><FromPoint /><ToPoint /></LIB></PocketFMSFlightplan>"
                )
            )
        );
    }

    [Fact]
    public void A_flightplan_without_metadata_is_parsed()
    {
        // <META> is not needed to read the route, and its absence used to be
        // dereferenced.
        var data = new PocketFmsFlightplanDeSerializer().DeSerialize(
            Wrap(
                "<PocketFMSFlightplan><LIB>"
                    + "<FromPoint><Latitude>51.1</Latitude><Longitude>14.9</Longitude></FromPoint>"
                    + "<ToPoint><Latitude>51.3</Latitude><Longitude>15.1</Longitude></ToPoint>"
                    + "</LIB></PocketFMSFlightplan>"
            )
        );

        Assert.NotNull(data);
        Assert.Equal(2, data!.Routes.Single().Waypoints.Count);
        Assert.Empty(data.Metadata);
    }
}
