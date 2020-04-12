using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class SimpleAudioSource : AudioSource
    {
        public SimpleAudioSource(AudioBuffer buffer)
        {
            AL10.alSourcei(Source, AL10.AL_BUFFER, (int)buffer.Buffer);
            Check();
        }
    }
}