using System;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class AudioBufferInt16Stereo : AudioBuffer
{
    public AudioBufferInt16Stereo(AL al, ReadOnlySpan<short> samples, int samplingRate) : base(al, samplingRate)
    {
        unsafe
        {
            fixed (short* samplePtr = &samples[0])
                AL.BufferData(Buffer, BufferFormat.Stereo16, samplePtr, samples.Length * sizeof(short), SamplingRate);
        }
        AL.Check();
        LastUpdatedDateTime = DateTime.Now;
        LastSize = samples.Length;
    }

    public void Update(ReadOnlySpan<short> samples)
    {
        unsafe
        {
            fixed (short* samplePtr = &samples[0])
                AL.BufferData(Buffer, BufferFormat.Stereo16, samplePtr, samples.Length * sizeof(short), SamplingRate);
        }
        AL.Check();
        LastUpdatedDateTime = DateTime.Now;
        LastSize = samples.Length;
    }
}