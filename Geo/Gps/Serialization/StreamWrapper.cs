using System.IO;

namespace Geo.Gps.Serialization;

public class StreamWrapper : Stream
{
    public StreamWrapper(Stream stream)
    {
        UnderlyingStream = stream.CanSeek ? stream : ConvertToMemoryStream(stream);
    }

    public Stream UnderlyingStream { get; }

    public override bool CanRead => UnderlyingStream.CanRead;

    public override bool CanSeek => UnderlyingStream.CanSeek;

    public override bool CanWrite => UnderlyingStream.CanWrite;

    public override long Length => UnderlyingStream.Length;

    public override long Position
    {
        get => UnderlyingStream.Position;
        set => UnderlyingStream.Position = value;
    }

    private MemoryStream ConvertToMemoryStream(Stream input)
    {
        var buffer = new byte[16 * 1024];
        var ms = new MemoryStream();
        int read;
        while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
            ms.Write(buffer, 0, read);
        return ms;
    }

    public override void Flush()
    {
        UnderlyingStream.Flush();
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        return UnderlyingStream.Read(buffer, offset, count);
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        return UnderlyingStream.Seek(offset, origin);
    }

    public override void SetLength(long value)
    {
        UnderlyingStream.SetLength(value);
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        UnderlyingStream.Write(buffer, offset, count);
    }
}
