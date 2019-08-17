using System.Runtime.InteropServices;

namespace UAlbion.Core
{
    [StructLayout(LayoutKind.Sequential)]
    public struct DepthCascadeLimits
    {
        public float NearLimit;
        public float MidLimit;
        public float FarLimit;
        readonly float _padding;
    }
}
