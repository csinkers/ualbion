using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public abstract class AudioBuffer : AudioObject, IDisposable
{
    public int SamplingRate { get; }
    public DateTime LastUpdatedDateTime { get; protected set; }
    public int LastUpdatedMs => (int)(DateTime.Now - LastUpdatedDateTime).TotalMilliseconds;
    public int LastSize { get; protected set; }

    internal readonly uint Buffer;
    bool _disposed;

    protected AudioBuffer(int samplingRate)
    {
        SamplingRate = samplingRate;
        AL10.alGenBuffers(1, out Buffer);
        Check();
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

        AL10.alDeleteBuffers(1, [Buffer]);
        Check();
        _disposed = true;
    }
}