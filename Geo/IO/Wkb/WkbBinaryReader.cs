using System;
using System.IO;

namespace Geo.IO.Wkb
{
    internal class WkbBinaryReader : IDisposable
    {
        private readonly BinaryReader _reader;
        private bool _disposed;

        public WkbBinaryReader(Stream stream)
        {
            _reader = new BinaryReader(stream);
            HasData = _reader.PeekChar() != -1;
            if (HasData)
                Encoding = (WkbEncoding)_reader.ReadByte();
        }

        public bool HasData { get; private set; }
        public WkbEncoding Encoding { get; private set; }

        public byte[] ReadBytes(int count)
        {
            var bytes = _reader.ReadBytes(count);
            if (Encoding == WkbEncoding.BigEndian)
                Array.Reverse(bytes);
            return bytes;
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

        public void Dispose()
        {
            if (_reader != null && !_disposed)
            {
                _disposed = true;
                _reader.Dispose();
            }
        }
    }
}