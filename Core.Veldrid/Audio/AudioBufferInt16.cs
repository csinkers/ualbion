using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioBufferInt16Stereo : AudioBuffer
    {
        public AudioBufferInt16Stereo(short[] samples, int samplingRate) : base(samplingRate)
        {
            AL10.alBufferData(Buffer, AL10.AL_FORMAT_STEREO16, samples, samples.Length * sizeof(short), SamplingRate);
            Check();
            LastUpdatedDateTime = DateTime.Now;
            LastSize = samples.Length;
        }

        public void Update(short[] samples)
        {
            AL10.alBufferData(Buffer, AL10.AL_FORMAT_STEREO16, samples, samples.Length * sizeof(short), SamplingRate);
            Check();
            LastUpdatedDateTime = DateTime.Now;
            LastSize = samples.Length;
        }
    }
}