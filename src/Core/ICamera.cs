using System.Numerics;
using UAlbion.Core.Visual;

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
        Vector3 ProjectWorldToNorm(Vector3 worldPosition);
        Vector3 UnprojectNormToWorld(Vector3 normPosition);
    }
}
