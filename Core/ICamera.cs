using System.Numerics;

namespace UAlbion.Core
{
    public interface ICamera : IComponent
    {
        Matrix4x4 ViewMatrix { get; }
        Matrix4x4 ProjectionMatrix { get; }
        Vector3 Position { get; }
        Vector3 LookDirection { get; }
        float NearDistance { get; }
        float FarDistance { get; }
        float FieldOfView { get; }
        float AspectRatio { get; }
        float Magnification { get; }
        CameraInfo GetCameraInfo();
    }
}
