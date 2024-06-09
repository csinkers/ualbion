using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual;

#pragma warning disable CA1051 // Do not declare visible instance fields
[SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not comparable")]
public struct BlendedSpriteInfo
{
    public override string ToString() => $"SID {Position}:{Size} ({Flags & ~SpriteFlags.DebugMask}) Z:{DebugZ}";

    // State
    public SpriteFlags Flags;
    public Vector4 Position;
    public Vector2 Size;

    public Vector2 TexPosition1; // Normalised texture coordinates
    public Vector2 TexSize1; // Normalised texture coordinates
    public uint TexLayer1;

    public Vector2 TexPosition2; // Normalised texture coordinates
    public Vector2 TexSize2; // Normalised texture coordinates
    public uint TexLayer2;

    // Derived properties for use by C# code
    public void OffsetBy(Vector3 offset) => Position += new Vector4(offset, 0);
    public int DebugZ => (int)((1.0f - Position.Z) * 4095);

    public BlendedSpriteInfo(SpriteFlags flags, Vector3 position, Vector2 size, Region region1, Region region2)
    {
        ArgumentNullException.ThrowIfNull(region1);
        Flags = flags;
        Position = new Vector4(position, 1);
        Size = size;
        TexPosition1 = region1.TexOffset;
        TexSize1 = region1.TexSize;
        TexLayer1 = (uint)region1.Layer;

        if (region2 != null)
        {
            TexPosition2 = region2.TexOffset;
            TexSize2 = region2.TexSize;
            TexLayer2 = (uint)region2.Layer;
        }
        else
        {
            TexPosition2 = region1.TexOffset;
            TexSize2 = region1.TexSize;
            TexLayer2 = (uint)region1.Layer;
        }
    }
}
#pragma warning restore CA1051 // Do not declare visible instance fields