using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geo.Geometries;
using Geo.Gps;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GpxSerializerAsyncTests
{
    private static GpsData SampleData()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(new Point(51.5, -0.12), "Home", "comment", "description"));
        return data;
    }

    [Fact]
    public async Task SerializeAsync_matches_the_synchronous_output()
    {
        var data = SampleData();
        var serializer = new Gpx11Serializer();

        using var stream = new MemoryStream();
        await serializer.SerializeAsync(data, stream);

        stream.Position = 0;
        var parsed = await GpsData.ParseAsync(stream);

        var waypoint = Assert.Single(parsed.Waypoints);
        Assert.Equal("Home", waypoint.Name);
        Assert.Equal(51.5, waypoint.Coordinate.Latitude);
    }

    [Fact]
    public async Task SerializeAsync_honours_a_cancelled_token()
    {
        using var stream = new MemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new Gpx11Serializer().SerializeAsync(SampleData(), stream, cts.Token)
        );
    }
}
