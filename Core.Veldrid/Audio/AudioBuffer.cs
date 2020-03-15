using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioBuffer : AudioObject, IDisposable
    {
        internal uint Buffer { get; private set; }
        public int SamplingRate { get; }

        public AudioBuffer( byte[] samples, int samplingRate)
        {
            SamplingRate = samplingRate;
            AL10.alGenBuffers(1, out var buffer); Check();
            AL10.alBufferData(buffer, AL10.AL_FORMAT_MONO8, samples, samples.Length, samplingRate); Check();
            Buffer = buffer;
        }

        protected AudioBuffer(short[] samples, int samplingRate)
        {
            SamplingRate = samplingRate;
            AL10.alGenBuffers(1, out var buffer); Check();
            AL10.alBufferData(buffer, AL10.AL_FORMAT_MONO16, samples, samples.Length, samplingRate); Check();
            Buffer = buffer;
        }

        public void Dispose()
        {
            AL10.alDeleteBuffers(1, new[] { Buffer });
            Check();
            Buffer = 0;
        }
    }
}
