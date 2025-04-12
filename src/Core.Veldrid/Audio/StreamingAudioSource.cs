using System;
using System.Linq;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class StreamingAudioSource : AudioSource
{
    public const int SamplingRate = 44100;
    const int BufferCount = 3;
    const int BufferSize = 16384;

    readonly uint[] _tempBufferIds = new uint[1];
    readonly short[] _tempBuffer = new short[BufferSize];
    readonly IAudioGenerator _generator;
    readonly AudioBufferInt16Stereo[] _buffers;
    bool _playing;

    public StreamingAudioSource(AL al, IAudioGenerator generator) : base(al)
    {
        _generator = generator ?? throw new ArgumentNullException(nameof(generator));
        _buffers = new AudioBufferInt16Stereo[BufferCount];

        short[] buffer = new short[BufferSize];
        for (int i = 0; i < _buffers.Length; i++)
        {
            _generator.FillBuffer(buffer);
            _buffers[i] = new AudioBufferInt16Stereo(AL, buffer, SamplingRate);
        }

        AL.SourceQueueBuffers(Source, _buffers.Select(x => x.Buffer).ToArray());
        AL.Check();
    }

    public override void Play() { _playing = true; base.Play(); }
    public override void Pause() { _playing = false; base.Pause(); }
    public override void Stop() { _playing = false; base.Stop(); }

    public void CycleBuffers() // Must be called periodically if streaming
    {
        if (State == SourceState.Playing != _playing)
        {
            if (_playing) Play();
            else Pause();
        }

        while (BuffersProcessed > BufferCount / 2)
        {
            AL.SourceUnqueueBuffers(Source, _tempBufferIds);

            _generator.FillBuffer(_tempBuffer);
            _buffers.First(x => x.Buffer == _tempBufferIds[0]).Update(_tempBuffer);

            AL.SourceQueueBuffers(Source, _tempBufferIds);
        }
    }

    protected override void Dispose(bool disposing)
    {
        Stop();

        if (disposing)
        {
            uint[] buffers = new uint[BuffersQueued];
            AL.SourceUnqueueBuffers(Source, buffers);

            foreach (var buffer in _buffers)
                buffer.Dispose();
        }

        base.Dispose(disposing);
    }
}