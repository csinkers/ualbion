using System;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public abstract class AudioBuffer : IDisposable
{
    public int SamplingRate { get; }
    public DateTime LastUpdatedDateTime { get; protected set; }
    public int LastUpdatedMs => (int)(DateTime.Now - LastUpdatedDateTime).TotalMilliseconds;
    public int LastSize { get; protected set; }

    internal readonly uint Buffer;
    bool _disposed;
    protected AL AL { get; private set; }

    protected AudioBuffer(AL al, int samplingRate)
    {
        AL = al;
        SamplingRate = samplingRate;
        Buffer = AL.GenBuffer();
        AL.Check();
        LastUpdatedDateTime = DateTime.Now;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    ~AudioBuffer() => Dispose(false);

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
            return;

        AL.DeleteBuffer(Buffer);
        AL.Check();
        _disposed = true;
    }
}