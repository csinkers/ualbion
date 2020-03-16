using System;
using System.Numerics;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioSource : AudioObject, IDisposable
    {
        readonly uint _source;
        readonly uint[] _buffers;

        public AudioSource(AudioBuffer buffer)
        {
            AL10.alGenSources(1, out _source); Check();
            AL10.alSourcei(_source, AL10.AL_BUFFER, (int)buffer.Buffer); Check();
        }

        public AudioSource()
        {
            /*
            const int bufferCount = 3;
            _buffers = new uint[bufferCount];
            AL10.alGenBuffers(bufferCount, _buffers);
            AL10.alSourceQueueBuffers(_source, bufferCount, _buffers);
            */
        }

        public void Play() { AL10.alSourcePlay(_source); Check(); }
        public void Pause() { AL10.alSourcePause(_source); Check(); }
        public void Stop() { AL10.alSourceStop(_source); Check(); }
        public void Rewind() {}

        public bool Looping { get => GetInt(AL10.AL_LOOPING) != AL10.AL_FALSE; set => SetInt(AL10.AL_LOOPING, value ? AL10.AL_TRUE : AL10.AL_FALSE); } 
        public Vector3 Position { get => GetVector(AL10.AL_POSITION); set => SetVector(AL10.AL_POSITION, value); } 
        public float Volume { get => GetFloat(AL10.AL_GAIN); set => SetFloat(AL10.AL_GAIN, value); } 
        public float Pitch { get => GetFloat(AL10.AL_PITCH); set => SetFloat(AL10.AL_PITCH, value); } 
        public float MaxDistance { get => GetFloat(AL10.AL_MAX_DISTANCE); set => SetFloat(AL10.AL_MAX_DISTANCE, value); } 
        public float RolloffFactor { get => GetFloat(AL10.AL_ROLLOFF_FACTOR); set => SetFloat(AL10.AL_ROLLOFF_FACTOR, value); } 
        public float ReferenceDistance { get => GetFloat(AL10.AL_REFERENCE_DISTANCE); set => SetFloat(AL10.AL_REFERENCE_DISTANCE, value); } 
        public float MinGain { get => GetFloat(AL10.AL_MIN_GAIN); set => SetFloat(AL10.AL_MIN_GAIN, value); } 
        public float MaxGain { get => GetFloat(AL10.AL_MAX_GAIN); set => SetFloat(AL10.AL_MAX_GAIN, value); } 
        public float ConeOuterGain { get => GetFloat(AL10.AL_CONE_OUTER_GAIN); set => SetFloat(AL10.AL_CONE_OUTER_GAIN, value); } 
        public float ConeInnerAngle { get => GetFloat(AL10.AL_CONE_INNER_ANGLE); set => SetFloat(AL10.AL_CONE_INNER_ANGLE, value); } 
        public float ConeOuterAngle { get => GetFloat(AL10.AL_CONE_OUTER_ANGLE); set => SetFloat(AL10.AL_CONE_OUTER_ANGLE, value); } 
        public Vector3 Direction { get => GetVector(AL10.AL_DIRECTION); set => SetVector(AL10.AL_DIRECTION, value); }
        public bool SourceRelative { get => GetInt(AL10.AL_SOURCE_RELATIVE) != AL10.AL_FALSE; set => SetInt(AL10.AL_SOURCE_RELATIVE, value ? AL10.AL_TRUE : AL10.AL_FALSE); }
        public SourceState State { get => (SourceState) GetInt(AL10.AL_SOURCE_STATE); set => SetInt(AL10.AL_SOURCE_STATE, (int) value); }
        public int BuffersQueued { get => GetInt(AL10.AL_BUFFERS_QUEUED); set => SetInt(AL10.AL_BUFFERS_QUEUED, value); }
        public int BuffersProcessed { get => GetInt(AL10.AL_BUFFERS_PROCESSED); set => SetInt(AL10.AL_BUFFERS_PROCESSED, value); }

        public SourceType SourceType
        {
            get => (SourceType)GetInt(AL10.AL_PITCH);
            set => SetInt(AL10.AL_PITCH, (int)value);
        }

        public void Dispose()
        {
            AL10.alDeleteSources(1, new[] { _source }); Check();
            if (_buffers != null)
            {
                // TODO: Ensure buffer not still in use, i.e. stop / detach.
                AL10.alDeleteBuffers(_buffers.Length, _buffers);
            }
        }

        #region Get / set helpers
        float GetFloat(int param) { AL10.alGetSourcef(_source, param, out var value); Check(); return value; }
        void SetFloat(int param, float value) { AL10.alSourcef(_source, param, value); Check(); }
        int GetInt(int param) { AL10.alGetSourcei(_source, param, out var value); Check(); return value; }
        void SetInt(int param, int value) { AL10.alSourcei(_source, param, value); Check(); }
        Vector3 GetVector(int param)
        {
            AL10.alGetSource3f(_source, param, out var x, out var y, out var z);
            Check();
            return new Vector3(x, y, z);
        }

        void SetVector(int param, Vector3 value)
        {
            AL10.alSource3f(_source, param, value.X, value.Y, value.Z);
            Check();
        }
        #endregion
    }
}