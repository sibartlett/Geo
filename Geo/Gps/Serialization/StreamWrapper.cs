using System.IO;

namespace Geo.Gps.Serialization
{
    public class StreamWrapper : Stream
    {
        public Stream UnderlyingStream { get; private set; }

        public StreamWrapper(Stream stream)
        {
            UnderlyingStream = stream.CanSeek ? stream : ConvertToMemoryStream(stream);
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

        public override bool CanRead
        {
            get { return UnderlyingStream.CanRead; }
        }

        public override bool CanSeek
        {
            get { return UnderlyingStream.CanSeek; }
        }

        public override bool CanWrite
        {
            get { return UnderlyingStream.CanWrite; }
        }

        public override long Length
        {
            get { return UnderlyingStream.Length; }
        }

        public override long Position {
            get { return UnderlyingStream.Position; }
            set{ UnderlyingStream.Position = value; }
        }
    }
}
