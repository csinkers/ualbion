using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioBufferUInt8 : AudioBuffer
    {
        public AudioBufferUInt8(ReadOnlySpan<byte> samples, int samplingRate) : base(samplingRate)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            unsafe
            {
                fixed (byte* samplePtr = &samples[0])
                    AL10.alBufferData(Buffer, AL10.AL_FORMAT_MONO8, (IntPtr) samplePtr, samples.Length, SamplingRate);
            }

            Check();
        }

        public void Update(ReadOnlySpan<byte> samples)
        {
            if (samples == null) throw new ArgumentNullException(nameof(samples));
            unsafe
            {
                fixed (byte* samplePtr = &samples[0])
                    AL10.alBufferData(Buffer, AL10.AL_FORMAT_MONO8, (IntPtr)samplePtr, samples.Length, SamplingRate);
            }
            Check();
            LastUpdatedDateTime = DateTime.Now;
            LastSize = samples.Length;
        }
    }
}