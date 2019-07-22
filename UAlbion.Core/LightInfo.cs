using System.Numerics;
using System.Runtime.InteropServices;

namespace UAlbion.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct LightInfo
    {
        public Vector3 Direction;
        readonly float _padding;
    }
}
