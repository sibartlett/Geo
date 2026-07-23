using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geo.Geometries;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkt;

public class WktReaderAsyncTests
{
    [Fact]
    public async Task ReadAsync_parses_a_geometry_from_a_stream()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("POINT (0.0 65.9)"));

        var geometry = await new WktReader().ReadAsync(stream);

        Assert.Equal(new Point(65.9, 0), geometry);
    }

    [Fact]
    public async Task ReadAsync_null_stream_throws_argument_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await new WktReader().ReadAsync(null)
        );
    }

    [Fact]
    public async Task ReadAsync_honours_a_cancelled_token()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("POINT (0.0 65.9)"));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new WktReader().ReadAsync(stream, cts.Token)
        );
    }
}
