using System;
using System.Numerics;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public abstract class AudioSource : AudioObject, IDisposable
    {
        protected readonly uint Source;
        bool _disposed;

        protected AudioSource()
        {
            AL10.alGenSources(1, out Source);
            Check();
        }

        public void Play() { AL10.alSourcePlay(Source); Check(); }
        public void Pause() { AL10.alSourcePause(Source); Check(); }
        public void Stop() { AL10.alSourceStop(Source); Check(); }
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
        public int BuffersQueued => GetInt(AL10.AL_BUFFERS_QUEUED);
        public int BuffersProcessed => GetInt(AL10.AL_BUFFERS_PROCESSED);
        // public float OffsetSeconds => GetFloat(AL11.AL_SEC_OFFSET);
        // public int OffsetSamples => GetFloat(AL11.AL_SAMPLE_OFFSET);

        public SourceType SourceType
        {
            get => (SourceType)GetInt(AL10.AL_SOURCE_TYPE);
            set => SetInt(AL10.AL_SOURCE_TYPE, (int)value);
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                throw new InvalidOperationException("AudioSource already disposed");
            AL10.alDeleteSources(1, new[] {Source});
            Check();
            _disposed = true;
        }

        #region Get / set helpers

        float GetFloat(int param) { AL10.alGetSourcef(Source, param, out var value); Check(); return value; }
        void SetFloat(int param, float value) { AL10.alSourcef(Source, param, value); Check(); }
        int GetInt(int param) { AL10.alGetSourcei(Source, param, out var value); Check(); return value; }
        void SetInt(int param, int value) { AL10.alSourcei(Source, param, value); Check(); }
        Vector3 GetVector(int param)
        {
            AL10.alGetSource3f(Source, param, out var x, out var y, out var z);
            Check();
            return new Vector3(x, y, z);
        }

        void SetVector(int param, Vector3 value)
        {
            AL10.alSource3f(Source, param, value.X, value.Y, value.Z);
            Check();
        }

        #endregion
    }
}