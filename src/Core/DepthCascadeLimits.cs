using System.Runtime.InteropServices;

namespace UAlbion.Core
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    [StructLayout(LayoutKind.Sequential)]
    public struct DepthCascadeLimits
    {
        public float NearLimit;
        public float MidLimit;
        public float FarLimit;
        readonly float _padding;
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}
