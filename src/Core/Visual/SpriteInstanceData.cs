using System;
using System.Diagnostics.CodeAnalysis;
using System.Numerics;
using UAlbion.Api.Visual;

namespace UAlbion.Core.Visual
{
#pragma warning disable CA1051 // Do not declare visible instance fields
    [SuppressMessage("Performance", "CA1815:Override equals and operator equals on value types", Justification = "Not comparable")]
    public partial struct SpriteInstanceData
    {
        public override string ToString() => $"SID {Position}:{TexLayer} ({Flags & ~SpriteFlags.DebugMask}) Z:{DebugZ}";

        // State
        public Vector4 Position;
        public Vector2 Size;
        public Vector2 TexPosition; // Normalised texture coordinates
        public Vector2 TexSize; // Normalised texture coordinates
        public uint TexLayer;
        public SpriteFlags Flags;

        // Derived properties for use by C# code
        public void OffsetBy(Vector3 offset) => Position += new Vector4(offset, 0);
        public int DebugZ => (int)((1.0f - Position.Z) * 4095);

        public SpriteInstanceData(Vector3 position, Vector2 size, Region region, SpriteFlags flags)
        {
            if (region == null) throw new ArgumentNullException(nameof(region));
            Position = new Vector4(position, 1);
            Size = size;
            TexPosition = region.TexOffset;
            TexSize = region.TexSize;
            TexLayer = (uint)region.Layer;
            Flags = flags;
        }

        /*
        static void BuildTransform(Vector3 position, Vector2 size, SpriteFlags flags, out Matrix4x4 transform)
        {
            var offset = (flags & SpriteFlags.AlignmentMask) switch
            {
                0                                                   => Vector3.Zero,
                SpriteFlags.MidAligned                              => new Vector3(0, -0.5f, 0),
                SpriteFlags.BottomAligned                           => new Vector3(0, -1.0f, 0),
                SpriteFlags.LeftAligned                             => new Vector3(0.5f, 0, 0),
                SpriteFlags.LeftAligned | SpriteFlags.MidAligned    => new Vector3(0.5f, -0.5f, 0),
                SpriteFlags.LeftAligned | SpriteFlags.BottomAligned => new Vector3(0.5f, -1.0f, 0),
                _ => Vector3.Zero
            };

            transform = Matrix4x4.CreateTranslation(offset);

            if ((flags & SpriteFlags.Floor) != 0)
            {
                transform *= new Matrix4x4(
                    1, 0, 0, 0,
                    0, 0,-1, 0,
                    0, 1, 0, 0,
                    0, 0, 0, 1);
            }

            transform *= Matrix4x4.CreateScale(new Vector3(size.X, size.Y, size.X));
            transform *= Matrix4x4.CreateTranslation(position);
        } */
    }
#pragma warning restore CA1051 // Do not declare visible instance fields
}