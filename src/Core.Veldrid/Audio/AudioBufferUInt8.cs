using System;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class AudioBufferUInt8 : AudioBuffer
{
    public AudioBufferUInt8(AL al, ReadOnlySpan<byte> samples, int samplingRate) : base(al, samplingRate)
    {
        unsafe
        {
            fixed (byte* samplePtr = &samples[0])
                AL.BufferData(Buffer, BufferFormat.Mono8, samplePtr, samples.Length, SamplingRate);
        }

        AL.Check();
        LastUpdatedDateTime = DateTime.Now;
        LastSize = samples.Length;
    }

    public void Update(ReadOnlySpan<byte> samples)
    {
        unsafe
        {
            fixed (byte* samplePtr = &samples[0])
                AL.BufferData(Buffer, BufferFormat.Mono8, samplePtr, samples.Length, SamplingRate);
        }
        AL.Check();
        LastUpdatedDateTime = DateTime.Now;
        LastSize = samples.Length;
    }
}