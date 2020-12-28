using System;
using System.Runtime.InteropServices;
using Veldrid;

namespace UAlbion.Core.Veldrid
{
    public static class VeldridUtil
    {
        public static void UpdateBufferSpan<T>(CommandList cl, DeviceBuffer buffer, ReadOnlySpan<T> instances) where T : unmanaged
        {
            if (cl == null) throw new ArgumentNullException(nameof(cl));
            unsafe
            {
                fixed (T* instancePtr = &instances[0])
                    cl.UpdateBuffer(
                        buffer,
                        0,
                        (IntPtr)instancePtr,
                        (uint)(instances.Length * Marshal.SizeOf<T>()));
            }
        }
    }
}
