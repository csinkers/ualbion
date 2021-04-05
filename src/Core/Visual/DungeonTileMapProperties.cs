using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace UAlbion.Core.Visual
{
#pragma warning disable CA1815 // Override equals and operator equals on value types
    [StructLayout(LayoutKind.Sequential)]
    public readonly struct DungeonTileMapProperties
    {
        public static readonly uint StructSize = (uint)Unsafe.SizeOf<DungeonTileMapProperties>();
        public Vector4 Scale { get; }
        public Vector4 Rotation { get; }
        public Vector4 Origin { get; }
        public Vector4 HorizontalSpacing { get; }
        public Vector4 VerticalSpacing { get; }
        public uint Width { get; }
        public uint AmbientLightLevel { get; }
        public uint FogColor { get; }
        public uint Pad1 { get; }

        public DungeonTileMapProperties(
            Vector3 scale,
            Vector3 rotation,
            Vector3 origin,
            Vector3 horizontalSpacing,
            Vector3 verticalSpacing,
            uint width,
            uint ambientLightLevel,
            uint fogColor)
        {
            Scale = new Vector4(scale, 1);
            Rotation = new Vector4(rotation, 1);
            Origin = new Vector4(origin, 1);
            HorizontalSpacing = new Vector4(horizontalSpacing, 1);
            VerticalSpacing = new Vector4(verticalSpacing, 1);
            Width = width;
            AmbientLightLevel = ambientLightLevel;
            FogColor = fogColor;
            Pad1 = 0;
        }
    }
#pragma warning restore CA1815 // Override equals and operator equals on value types
}