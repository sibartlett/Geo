using System;
using System.IO;

namespace Geo.IO.Wkb;

internal class WkbBinaryWriter : IDisposable
{
    private readonly BinaryWriter _writer;
    private bool _disposed;

    public WkbBinaryWriter(Stream stream, WkbEncoding encoding)
    {
        Encoding = encoding;
        _writer = new BinaryWriter(stream);
    }

    public WkbEncoding Encoding { get; }

    public void Dispose()
    {
        if (_writer != null && !_disposed)
        {
            _disposed = true;
            _writer.Dispose();
        }
    }

    public void Write(byte value)
    {
        _writer.Write(value);
    }

    public void Write(byte[] bytes)
    {
        if (Encoding == WkbEncoding.BigEndian)
            Array.Reverse(bytes);
        _writer.Write(bytes);
    }

    public void Write(double value)
    {
        var bytes = BitConverter.GetBytes(value);
        Write(bytes);
    }

    public void Write(int value)
    {
        var bytes = BitConverter.GetBytes(value);
        Write(bytes);
    }

    public void Write(short value)
    {
        var bytes = BitConverter.GetBytes(value);
        Write(bytes);
    }

    public void Write(long value)
    {
        var bytes = BitConverter.GetBytes(value);
        Write(bytes);
    }

    public void Write(uint value)
    {
        var bytes = BitConverter.GetBytes(value);
        Write(bytes);
    }
}
