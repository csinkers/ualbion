using System;
using System.IO;
using System.Text;
using SerdesNet;
using UAlbion.Api;

namespace UAlbion.Formats;

public static class AlbionSerdes
{
    public static MemoryReaderSerdes CreateReader(ReadOnlyMemory<byte> buffer, Action disposeAction = null)
        => new(buffer, ApiUtil.Assert, disposeAction);

    public static ReaderSerdes CreateReader(Stream stream, int? length = null, Action disposeAction = null)
    {
        var br = new BinaryReader(stream);
        return new ReaderSerdes(br, length ?? br.BaseStream.Length, ApiUtil.Assert, Disposer);

        void Disposer()
        {
            br.Dispose();
            disposeAction?.Invoke();
        }
    }

    public static ReaderSerdes CreateReaderKeepOpen(Stream stream, int? length = null, Action disposeAction = null)
    {
        var br = new BinaryReader(stream, Encoding.ASCII, true);
        return new ReaderSerdes(br, length ?? br.BaseStream.Length, ApiUtil.Assert, disposeAction);
    }

    public static ReaderSerdes CreateReader(BinaryReader br, int? length = null, Action disposeAction = null)
    {
        return new ReaderSerdes(br, length ?? br.BaseStream.Length, ApiUtil.Assert, Disposer);

        void Disposer()
        {
            br.Dispose();
            disposeAction?.Invoke();
        }
    }

    public static MemoryWriterSerdes CreateWriter(int capacity = 0, Action disposeAction = null)
        => new(capacity, ApiUtil.Assert, disposeAction);

    public static WriterSerdes CreateWriter(Stream stream, Action disposeAction = null)
    {
        var bw = new BinaryWriter(stream);
        return new WriterSerdes(bw, ApiUtil.Assert, Disposer);

        void Disposer()
        {
            bw.Dispose();
            disposeAction?.Invoke();
        }
    }

}