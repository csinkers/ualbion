using System.Numerics;
using System.Runtime.InteropServices;
using Veldrid;

namespace UAlbion.Core
{
    public interface ICamera : IComponent
    {
        void UpdateBackend(GraphicsDevice gd);
        Matrix4x4 ViewMatrix { get; }
        Matrix4x4 ProjectionMatrix { get; }
        Vector3 Position { get; set; }
        Vector3 LookDirection { get; }
        float FarDistance { get; }
        float FieldOfView { get; }
        float NearDistance { get; }
        float AspectRatio { get; }
        float Magnification { get; set; }
        CameraInfo GetCameraInfo();
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        readonly float _padding1;
        public Vector3 CameraLookDirection;
        readonly float _padding2;
    }
}
