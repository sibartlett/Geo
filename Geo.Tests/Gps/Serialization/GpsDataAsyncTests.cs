using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Geo.Geometries;
using Geo.Gps;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class GpsDataAsyncTests : SerializerTestFixtureBase
{
    // A read-only, non-seekable stream, forcing ParseAsync to buffer the source
    // itself rather than relying on the underlying stream being seekable.
    private class NonSeekableStream : Stream
    {
        private readonly Stream _inner;

        public NonSeekableStream(byte[] data)
        {
            _inner = new MemoryStream(data);
        }

        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new System.NotSupportedException();

        public override long Position
        {
            get => throw new System.NotSupportedException();
            set => throw new System.NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count) =>
            _inner.Read(buffer, offset, count);

        public override void Flush() { }

        public override long Seek(long offset, SeekOrigin origin) =>
            throw new System.NotSupportedException();

        public override void SetLength(long value) => throw new System.NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) =>
            throw new System.NotSupportedException();
    }

    private static GpsData SampleData()
    {
        var data = new GpsData();
        data.Waypoints.Add(new Waypoint(new Point(51.5, -0.12), "Home", "comment", "description"));
        return data;
    }

    [Theory]
    [InlineData("gpx", "Bergamo to Manchester.gpx")]
    [InlineData("igc", "igc2.igc")]
    [InlineData("nmea", "Stockholm_Walk.nmea")]
    [InlineData("garmin", "garmin.fpl")]
    [InlineData("skydemon", "skydemon.flightplan")]
    [InlineData("pocketfms", "pocketfms_fp.xml")]
    public async Task ParseAsync_detects_the_format_and_returns_data(
        string subDirectory,
        string fileName
    )
    {
        var file = GetReferenceFileDirectory(subDirectory)
            .GetFiles()
            .First(x => x.Name == fileName);
        using var stream = file.OpenRead();

        var data = await GpsData.ParseAsync(stream);

        Assert.NotNull(data);
    }

    [Fact]
    public async Task ParseAsync_returns_null_for_unrecognised_content()
    {
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes("this is not a gps file"));

        Assert.Null(await GpsData.ParseAsync(stream));
    }

    [Fact]
    public async Task ParseAsync_buffers_a_non_seekable_stream()
    {
        var gpx = SampleData().ToGpx();
        using var stream = new NonSeekableStream(Encoding.UTF8.GetBytes(gpx));

        var parsed = await GpsData.ParseAsync(stream);

        var waypoint = Assert.Single(parsed.Waypoints);
        Assert.Equal(51.5, waypoint.Coordinate.Latitude);
        Assert.Equal(-0.12, waypoint.Coordinate.Longitude);
        Assert.Equal("Home", waypoint.Name);
    }

    [Fact]
    public async Task ParseAsync_round_trips_through_ToGpx()
    {
        var gpx = SampleData().ToGpx();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        var parsed = await GpsData.ParseAsync(stream);

        var waypoint = Assert.Single(parsed.Waypoints);
        Assert.Equal("Home", waypoint.Name);
    }

    [Fact]
    public async Task ParseAsync_honours_a_cancelled_token()
    {
        var gpx = SampleData().ToGpx();
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(gpx));
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        await Assert.ThrowsAnyAsync<System.OperationCanceledException>(async () =>
            await GpsData.ParseAsync(stream, cts.Token)
        );
    }
}
