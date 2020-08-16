using System.Numerics;
using OpenAL;

namespace UAlbion.Core.Veldrid.Audio
{
    public class AudioListener : AudioObject
    {
        // ReSharper disable UnusedMember.Global
        #pragma warning disable CA1822 // Can be static
        public float Gain
        {
            get { AL10.alGetListenerf(AL10.AL_GAIN, out var gain); Check(); return gain; }
            set { AL10.alListenerf(AL10.AL_GAIN, value); Check(); }
        }

        public Vector3 Position
        {
            get { AL10.alGetListener3f(AL10.AL_POSITION, out var x, out var y, out var z); Check(); return new Vector3(x, y, z); }
            set { AL10.alListener3f(AL10.AL_POSITION, value.X, value.Y, value.Z); Check(); }
        }

        public Vector3 Velocity
        {
            get { AL10.alGetListener3f(AL10.AL_VELOCITY, out var x, out var y, out var z); Check(); return new Vector3(x, y, z); }
            set { AL10.alListener3f(AL10.AL_VELOCITY, value.X, value.Y, value.Z); Check(); }
        }

        public (Vector3, Vector3) Orientation // (PointAt, Up)
        {
            get
            {
                float[] values = new float[6];
                AL10.alGetListenerfv(AL10.AL_ORIENTATION, values);
                Check();
                return (
                    new Vector3(values[0], values[1], values[2]),
                    new Vector3(values[3], values[4], values[5])
                );
            }
            set
            {
                var values = new[]
                {
                    value.Item1.X, value.Item1.Y, value.Item1.Z,
                    value.Item2.X, value.Item2.Y, value.Item2.Z
                };
                AL10.alListenerfv(AL10.AL_ORIENTATION, values);
                Check();
            }
        }
        // ReSharper restore UnusedMember.Global
        #pragma warning restore CA1822 // Can be static
    }
}