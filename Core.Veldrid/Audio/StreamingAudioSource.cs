using System;
using System.Linq;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class StreamingAudioSource : AudioSource
    {
        public const int SamplingRate = 44100;
        const int BufferCount = 3;
        const int BufferSize = 16384;

        readonly IAudioGenerator _generator;
        readonly AudioBufferInt16Stereo[] _buffers;

        public StreamingAudioSource(IAudioGenerator generator)
        {
            _generator = generator ?? throw new ArgumentNullException(nameof(generator));
            _buffers = new AudioBufferInt16Stereo[BufferCount];

            short[] buffer = new short[BufferSize];
            for (int i = 0; i < _buffers.Length; i++)
            {
                _generator.FillBuffer(buffer);
                _buffers[i] = new AudioBufferInt16Stereo(buffer, SamplingRate);
            }

            AL10.alSourceQueueBuffers(Source, BufferCount, _buffers.Select(x => x.Buffer).ToArray());
            Check();
        }

        public void CycleBuffers() // Must be called periodically if streaming
        {
            while (BuffersProcessed > BufferCount / 2)
            {
                uint[] bufferIds = new uint[1];
                AL10.alSourceUnqueueBuffers(Source, 1, bufferIds);

                short[] buffer = new short[BufferSize];
                int length = _generator.FillBuffer(buffer);
                _buffers.First(x => x.Buffer == bufferIds[0]).Update(buffer);

                AL10.alSourceQueueBuffers(Source, 1, bufferIds);
            }
        }

        protected override void Dispose(bool disposing)
        {
            Stop();
            if (disposing)
            {
                uint[] buffers = new uint[BuffersQueued];
                AL10.alSourceUnqueueBuffers(Source, buffers.Length, buffers);
                foreach (var buffer in _buffers)
                    buffer.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}