#nullable enable
using System;
using System.IO;

namespace Geo.IO.Wkb;

internal class WkbBinaryReader : IDisposable
{
    private readonly BinaryReader _reader;
    private int _pushback;
    private bool _disposed;

    public WkbBinaryReader(Stream stream)
    {
        _reader = new BinaryReader(stream);
        // BinaryReader.PeekChar cannot be used to test for data here: it returns -1 for
        // any stream that cannot seek (a network, pipe or compression stream), which
        // would silently turn a perfectly good geometry into "no data". It also decodes
        // bytes as text, which WKB is not. Read the first byte instead and hold it back
        // for the first subsequent read.
        _pushback = stream.ReadByte();
        HasData = _pushback != -1;
    }

    public bool HasData { get; }
    public WkbEncoding Encoding { get; private set; }

    public void Dispose()
    {
        if (!_disposed)
        {
            _disposed = true;
            _reader.Dispose();
        }
    }

    public WkbEncoding ReadAndSetEncoding()
    {
        Encoding = (WkbEncoding)ReadByte();
        return Encoding;
    }

    public byte[] ReadBytes(int count)
    {
        var bytes = new byte[count];
        var offset = 0;

        if (_pushback >= 0 && count > 0)
        {
            bytes[0] = (byte)_pushback;
            _pushback = -1;
            offset = 1;
        }

        // A single read can return fewer bytes than asked for without being at the end
        // of the stream (network and compression streams routinely do), so keep reading
        // until the request is satisfied. A read of zero bytes means the stream really
        // has ended: surface that as EndOfStreamException so WkbReader can translate a
        // truncated geometry into a SerializationException.
        while (offset < count)
        {
            var read = _reader.Read(bytes, offset, count - offset);
            if (read <= 0)
                throw new EndOfStreamException();
            offset += read;
        }

        if (Encoding == WkbEncoding.BigEndian)
            Array.Reverse(bytes);
        return bytes;
    }

    private byte ReadByte()
    {
        if (_pushback < 0)
            return _reader.ReadByte();

        var value = (byte)_pushback;
        _pushback = -1;
        return value;
    }

    public double ReadDouble()
    {
        var bytes = ReadBytes(8);
        return BitConverter.ToDouble(bytes, 0);
    }

    public int ReadInt32()
    {
        var bytes = ReadBytes(4);
        return BitConverter.ToInt32(bytes, 0);
    }

    public short ReadInt16()
    {
        var bytes = ReadBytes(2);
        return BitConverter.ToInt16(bytes, 0);
    }

    public long ReadInt64()
    {
        var bytes = ReadBytes(8);
        return BitConverter.ToInt64(bytes, 0);
    }

    public uint ReadUInt32()
    {
        var bytes = ReadBytes(4);
        return BitConverter.ToUInt32(bytes, 0);
    }
}
