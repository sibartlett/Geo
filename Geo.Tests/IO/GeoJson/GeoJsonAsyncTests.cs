using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geo.Geometries;
using Geo.IO.GeoJson;
using Xunit;

namespace Geo.Tests.IO.GeoJson;

public class GeoJsonAsyncTests
{
    [Fact]
    public async Task ReadAsync_parses_geojson_from_a_stream()
    {
        var json = new Point(0, 0).ToGeoJson();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));

        var result = await new GeoJsonReader().ReadAsync(stream);

        Assert.Equal(new Point(0, 0), result);
    }

    [Fact]
    public async Task ReadAsync_null_stream_throws_argument_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await new GeoJsonReader().ReadAsync(null)
        );
    }

    [Fact]
    public async Task ReadAsync_honours_a_cancelled_token()
    {
        var json = new Point(0, 0).ToGeoJson();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(json));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new GeoJsonReader().ReadAsync(stream, cts.Token)
        );
    }
}
