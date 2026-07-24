using System.IO;
using System.Text;
using System.Threading.Tasks;
using Geo.Gps.Serialization;
using Xunit;

namespace Geo.Tests.Gps.Serialization;

public class StreamWrapperTests
{
    private static byte[] Bytes(string text) => Encoding.UTF8.GetBytes(text);

    [Fact]
    public void Seekable_stream_is_used_directly()
    {
        var inner = new MemoryStream(Bytes("hello"));
        var wrapper = new StreamWrapper(inner);

        Assert.Same(inner, wrapper.UnderlyingStream);
        Assert.True(wrapper.CanSeek);
    }

    [Fact]
    public void Non_seekable_stream_is_buffered_into_a_seekable_stream()
    {
        var inner = new NonSeekableStream(Bytes("buffered content"));
        var wrapper = new StreamWrapper(inner);

        // The non-seekable source must have been copied into a seekable buffer.
        Assert.NotSame(inner, wrapper.UnderlyingStream);
        Assert.True(wrapper.CanSeek);

        // And the buffered content must be readable from the start.
        wrapper.Seek(0, SeekOrigin.Begin);
        using var reader = new StreamReader(wrapper, Encoding.UTF8, false, 1024, true);
        Assert.Equal("buffered content", reader.ReadToEnd());
    }

    [Fact]
    public async Task CreateAsync_buffers_the_source_and_rewinds_to_the_start()
    {
        var inner = new NonSeekableStream(Bytes("async content"));

        var wrapper = await StreamWrapper.CreateAsync(inner);

        Assert.True(wrapper.CanSeek);
        Assert.Equal(0, wrapper.Position);
        using var reader = new StreamReader(wrapper, Encoding.UTF8, false, 1024, true);
        Assert.Equal("async content", reader.ReadToEnd());
    }

    [Fact]
    public void Stream_members_delegate_to_the_underlying_stream()
    {
        var inner = new MemoryStream();
        var wrapper = new StreamWrapper(inner);

        Assert.True(wrapper.CanRead);
        Assert.True(wrapper.CanWrite);

        var payload = Bytes("payload");
        wrapper.Write(payload, 0, payload.Length);
        wrapper.Flush();
        Assert.Equal(payload.Length, wrapper.Length);

        wrapper.Position = 0;
        Assert.Equal(0, wrapper.Position);

        var buffer = new byte[payload.Length];
        var read = wrapper.Read(buffer, 0, buffer.Length);
        Assert.Equal(payload.Length, read);
        Assert.Equal("payload", Encoding.UTF8.GetString(buffer));

        wrapper.SetLength(3);
        Assert.Equal(3, wrapper.Length);
    }

    // A read-only stream that reports CanSeek == false, forcing StreamWrapper to
    // buffer it into memory.
    private sealed class NonSeekableStream : Stream
    {
        private readonly MemoryStream _inner;

        public NonSeekableStream(byte[] data) => _inner = new MemoryStream(data);

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
}
