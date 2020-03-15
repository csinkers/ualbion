using System;
using System.Numerics;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioSource : AudioObject, IDisposable
    {
        readonly uint _source;

        public AudioSource(AudioBuffer buffer)
        {
            AL10.alGenSources(1, out _source); Check();
            AL10.alSourcei(_source, AL10.AL_BUFFER, (int)buffer.Buffer); Check();
        }

        public void Play() { AL10.alSourcePlay(_source); Check(); }
        public void Stop() { AL10.alSourceStop(_source); Check(); }

        public bool Looping
        {
            get { AL10.alGetSourcei(_source, AL10.AL_LOOPING, out var value); Check(); return value != AL10.AL_FALSE; }
            set { AL10.alSourcei(_source, AL10.AL_LOOPING, value ? AL10.AL_TRUE : AL10.AL_FALSE); Check(); }
        }

        public Vector3 Position
        {
            get { AL10.alGetSource3f(_source, AL10.AL_POSITION, out var x, out var y, out var z); Check(); return new Vector3(x, y, z); }
            set { AL10.alSource3f(_source, AL10.AL_POSITION, value.X, value.Y, value.Z); Check(); }
        }

        public float Volume
        {
            get { AL10.alGetSourcef(_source, AL10.AL_GAIN, out var gain); Check(); return gain; }
            set { AL10.alSourcef(_source, AL10.AL_GAIN, value); Check(); }
        }

        public void Dispose()
        {
            AL10.alDeleteSources(1, new[] { _source }); Check();
        }
    }

    public class AudioException : Exception
    {
        public AudioException(string message) : base(message) { }
    }
}