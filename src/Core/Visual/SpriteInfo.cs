using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;
#pragma warning disable CA1051 // Do not declare visible instance fields
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not comparable")]
public struct SpriteInfo
{
    public override string ToString() => $"SID {Position}:{TexLayer} ({Flags & ~SpriteFlags.DebugMask}) Z:{DebugZ}";

    // Note: This struct layout should exactly match GpuSpriteInstanceData in UAlbion.Core.Veldrid
    public SpriteFlags Flags;
    public Vector4 Position;
    public Vector2 Size;
    public Vector2 TexPosition; // Normalised texture coordinates
    public Vector2 TexSize; // Normalised texture coordinates
    public uint TexLayer;

    // Derived properties for use by C# code
    public void OffsetBy(Vector3 offset) => Position += new Vector4(offset, 0);
    public int DebugZ => (int)((1.0f - Position.Z) * 4095);

    public SpriteInfo(SpriteFlags flags, Vector3 position, Vector2 size, Region region)
    {
        ArgumentNullException.ThrowIfNull(region);
        Flags = flags;
        Position = new Vector4(position, 1);
        Size = size;
        TexPosition = region.TexOffset;
        TexSize = region.TexSize;
        TexLayer = (uint)region.Layer;
    }
}
#pragma warning restore CA1051 // Do not declare visible instance fields
