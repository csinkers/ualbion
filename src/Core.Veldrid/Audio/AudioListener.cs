using System.Numerics;
using Silk.NET.OpenAL;

namespace UAlbion.Core.Veldrid.Audio;

public class AudioListener(AL al)
{
    // ReSharper disable UnusedMember.Global
    public float Gain
    {
        get { al.GetListenerProperty(ListenerFloat.Gain, out var gain); al.Check(); return gain; }
        set { al.SetListenerProperty(ListenerFloat.Gain, value); al.Check(); }
    }

    public Vector3 Position
    {
        get { al.GetListenerProperty(ListenerVector3.Position, out var x, out var y, out var z); al.Check(); return new Vector3(x, y, z); }
        set { al.SetListenerProperty(ListenerVector3.Position, value.X, value.Y, value.Z); al.Check(); }
    }

    public Vector3 Velocity
    {
        get { al.GetListenerProperty(ListenerVector3.Velocity, out var x, out var y, out var z); al.Check(); return new Vector3(x, y, z); }
        set { al.SetListenerProperty(ListenerVector3.Velocity, value.X, value.Y, value.Z); al.Check(); }
    }

    public unsafe (Vector3, Vector3) Orientation // (PointAt, Up)
    {
        get
        {
            float[] values = new float[6];
            fixed (float* p = &values[0])
                al.GetListenerProperty(ListenerFloatArray.Orientation, p);

            al.Check();

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

            fixed (float* p = &values[0])
                al.SetListenerProperty(ListenerFloatArray.Orientation, p);

            al.Check();
        }
    }
    // ReSharper restore UnusedMember.Global
}