using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Geo.Abstractions.Interfaces;
using Geo.IO.Wkb;
using Geo.IO.Wkt;
using Xunit;

namespace Geo.Tests.IO.Wkb;

public class WkbAsyncTests
{
    private static IGeometry Sample() => new WktReader().Read("POINT (45.89 23.9)");

    [Fact]
    public async Task ReadAsync_round_trips_a_geometry()
    {
        var geometry = Sample();
        var wkb = new WkbWriter().Write(geometry);

        using var stream = new MemoryStream(wkb);
        var result = await new WkbReader().ReadAsync(stream);

        Assert.Equal(geometry, result);
    }

    [Fact]
    public async Task WriteAsync_round_trips_a_geometry()
    {
        var geometry = Sample();

        using var stream = new MemoryStream();
        await new WkbWriter().WriteAsync(geometry, stream);

        stream.Position = 0;
        Assert.Equal(geometry, new WkbReader().Read(stream));
    }

    // Regression: Write(geometry, stream) previously disposed the temporary
    // stream (via BinaryWriter) before copying and never rewound it, so it
    // wrote nothing and/or threw. It must now round-trip like the byte[] path.
    [Fact]
    public void Write_to_stream_round_trips_a_geometry()
    {
        var geometry = Sample();

        using var stream = new MemoryStream();
        new WkbWriter().Write(geometry, stream);

        Assert.Equal(new WkbWriter().Write(geometry).Length, stream.Length);
        stream.Position = 0;
        Assert.Equal(geometry, new WkbReader().Read(stream));
    }

    [Fact]
    public async Task ReadAsync_null_stream_throws_argument_null()
    {
        await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            await new WkbReader().ReadAsync(null)
        );
    }

    [Fact]
    public async Task ReadAsync_honours_a_cancelled_token()
    {
        var wkb = new WkbWriter().Write(Sample());
        using var stream = new MemoryStream(wkb);
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new WkbReader().ReadAsync(stream, cts.Token)
        );
    }

    [Fact]
    public async Task WriteAsync_honours_a_cancelled_token()
    {
        using var stream = new MemoryStream();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(async () =>
            await new WkbWriter().WriteAsync(Sample(), stream, cts.Token)
        );
    }
}
