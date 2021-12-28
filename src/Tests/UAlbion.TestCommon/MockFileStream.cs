using System;
using System.IO;

namespace UAlbion.TestCommon;

public class MockFileStream : Stream
{
    readonly MemoryStream _stream;
    readonly bool _readOnly;
    long _offset;

    public MockFileStream(MemoryStream stream, bool readOnly = false)
    {
        _stream = stream ?? throw new ArgumentNullException(nameof(stream));
        _readOnly = readOnly;
    }

    public override int Read(byte[] buffer, int offset, int count)
    {
        if (_stream.Position != _offset) _stream.Position = _offset; 
        var result = _stream.Read(buffer, offset, count);
        _offset = _stream.Position;
        return result;
    }

    public override void Write(byte[] buffer, int offset, int count)
    {
        if (_stream.Position != _offset) _stream.Position = _offset;
        _stream.Write(buffer, offset, count);
        _offset = _stream.Position;
    }

    public override long Seek(long offset, SeekOrigin origin)
    {
        var result = _stream.Seek(offset, origin);
        _offset = _stream.Position;
        return result;
    }

    public override void SetLength(long value) => _stream.SetLength(value);
    public override void Flush() => _stream.Flush();
    public override bool CanRead => _stream.CanRead;
    public override bool CanSeek => _stream.CanSeek;
    public override bool CanWrite => !_readOnly && _stream.CanWrite;
    public override bool CanTimeout => _stream.CanTimeout;
    public override int ReadTimeout { get => _stream.ReadTimeout; set => _stream.ReadTimeout = value; }
    public override int WriteTimeout { get => _stream.WriteTimeout; set => _stream.WriteTimeout = value; }
    public override long Length => _stream.Length;
    public override long Position { get => _offset; set => _offset = value; }
}