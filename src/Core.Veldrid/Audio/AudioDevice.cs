using System;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public sealed class AudioDevice : IDisposable
{
    readonly AL _al;
    readonly ALContext _alc;
    readonly unsafe Device* _device;
    readonly unsafe Context* _context;
    bool _disposed;

    public unsafe AudioDevice()
    {
        _alc = ALContext.GetApi();
        _al = AL.GetApi();
        _device = _alc.OpenDevice("");
        if (_device == null)
            throw new AudioException("Failed to open audio device.");

        _context = _alc.CreateContext(_device, null);
        _alc.MakeContextCurrent(_context);
        _al.Check();
        Listener = new(_al);
    }

    public AudioListener Listener { get; }

    public float DopplerFactor
    {
        get { var value = _al.GetStateProperty(StateFloat.DopplerFactor); _al.Check(); return value; }
        set { _al.DopplerFactor(value); _al.Check(); }
    }

    public DistanceModel DistanceModel
    {
        get { var value = _al.GetStateProperty(StateInteger.DistanceModel); _al.Check(); return (DistanceModel)value; }
        set { _al.DistanceModel(value); _al.Check(); }
    }

    public AudioBufferUInt8 CreateBuffer(ReadOnlySpan<byte> samples, int sampleRate) => new(_al, samples, sampleRate);
    public SimpleAudioSource CreateSource(AudioBuffer buffer) => new(_al, buffer);
    public StreamingAudioSource CreateStreamingSource(IAudioGenerator generator) => new(_al, generator);

    public void Dispose()
    {
        InnerDispose();
        GC.SuppressFinalize(this);
    }

    ~AudioDevice() => InnerDispose();

    unsafe void InnerDispose()
    {
        if (_disposed)
            return;

        _alc.DestroyContext(_context);
        _alc.CloseDevice(_device);
        _al.Dispose();
        _alc.Dispose();
        _disposed = true;
    }
}