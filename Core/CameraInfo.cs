using System.Numerics;
using System.Runtime.InteropServices;

namespace UAlbion.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct CameraInfo
    {
        public Vector3 CameraPosition_WorldSpace;
        readonly float _padding1;
        public Vector3 CameraLookDirection;
        readonly float _padding2;
    }
}