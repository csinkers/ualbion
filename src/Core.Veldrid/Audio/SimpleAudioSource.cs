using System;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class SimpleAudioSource : AudioSource
{
    public SimpleAudioSource(AL al, AudioBuffer buffer): base(al)
    {
        ArgumentNullException.ThrowIfNull(buffer);

        AL.SetSourceProperty(Source, SourceInteger.Buffer, buffer.Buffer);
        AL.Check();
    }
}