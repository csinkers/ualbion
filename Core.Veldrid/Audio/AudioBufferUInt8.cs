using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioBufferUInt8 : AudioBuffer
    {
        public AudioBufferUInt8(byte[] samples, int samplingRate) : base(samplingRate)
        {
            AL10.alBufferData(Buffer, AL10.AL_FORMAT_MONO8, samples, samples.Length, SamplingRate);
            Check();
        }

        public void Update(byte[] samples)
        {
            AL10.alBufferData(Buffer, AL10.AL_FORMAT_MONO8, samples, samples.Length, SamplingRate);
            Check();
            LastUpdatedDateTime = DateTime.Now;
            LastSize = samples.Length;
        }
    }
}