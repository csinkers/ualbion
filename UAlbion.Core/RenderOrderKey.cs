using System;
using System.Runtime.CompilerServices;

namespace UAlbion.Core
{
    public struct RenderOrderKey : IComparable<RenderOrderKey>, IComparable
    {
        public readonly ulong Value;

        public RenderOrderKey(ulong value) { Value = value; }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderOrderKey Create(int materialId, float cameraDistance) => Create((uint)materialId, cameraDistance);

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RenderOrderKey Create(uint materialId, float cameraDistance)
        {
            uint cameraDistanceInt = (uint)Math.Min(uint.MaxValue, (cameraDistance * 1000f));

            return new RenderOrderKey(
                ((ulong)materialId << 32) +
                cameraDistanceInt);
        }

        public int CompareTo(RenderOrderKey other) { return Value.CompareTo(other.Value); } 
        int IComparable.CompareTo(object obj) { return Value.CompareTo(obj); }
    }
}