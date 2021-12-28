using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class SimpleAudioSource : AudioSource
{
    public SimpleAudioSource(AudioBuffer buffer)
    {
        if (buffer == null) throw new ArgumentNullException(nameof(buffer));
        AL10.alSourcei(Source, AL10.AL_BUFFER, (int)buffer.Buffer);
        Check();
    }
}