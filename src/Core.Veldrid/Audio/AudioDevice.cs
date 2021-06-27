using System;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public sealed class AudioDevice : IDisposable
    {
        readonly IntPtr _device;
        readonly IntPtr _context;
        bool _disposed;

        void Check()
        {
            int error = ALC10.alcGetError(_device);
            switch(error)
            {
                case ALC10.ALC_NO_ERROR: return;
                case ALC10.ALC_INVALID_DEVICE: throw new AudioException("a bad device was passed to an OpenAL function");
                case ALC10.ALC_INVALID_CONTEXT: throw new AudioException("a bad context was passed to an OpenAL function");
                case ALC10.ALC_INVALID_ENUM: throw new AudioException("an unknown enum value was passed to an OpenAL function");
                case ALC10.ALC_INVALID_VALUE: throw new AudioException("an invalid value was passed to an OpenAL function");
                case ALC10.ALC_OUT_OF_MEMORY: throw new AudioException("the requested operation resulted in OpenAL running out of memory");
            }
        }

        public AudioDevice()
        {
            _device = ALC10.alcOpenDevice(null); Check();
            _context = ALC10.alcCreateContext(_device, Array.Empty<int>()); Check();
            ALC10.alcMakeContextCurrent(_context); Check();
            var _ = AL10.alGetError(); // Clear error code for subsequent callers
        }

        public AudioListener Listener { get; } = new();

        public float DopplerFactor
        {
            get { var value = AL10.alGetFloat(AL10.AL_DOPPLER_FACTOR); Check(); return value; }
            set { AL10.alDopplerFactor(value); Check(); }
        }

        public DistanceModel DistanceModel
        {
            get { var value = AL10.alGetInteger(AL10.AL_DOPPLER_FACTOR); Check(); return (DistanceModel)value; }
            set { AL10.alDistanceModel((int)value); Check(); }
        }

        public void Dispose()
        {
            InnerDispose();
            GC.SuppressFinalize(this);
        }

        ~AudioDevice() => InnerDispose();

        void InnerDispose()
        {
            if (_disposed)
                return;

            if (_context != IntPtr.Zero)
            {
                ALC10.alcMakeContextCurrent(IntPtr.Zero); Check();
                ALC10.alcDestroyContext(_context); Check();
            }

            if (_device != IntPtr.Zero)
            {
                ALC10.alcCloseDevice(_device);
                Check();
            }

            _disposed = true;
        }
    }
}