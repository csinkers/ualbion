using System;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UAlbion.Core.Visual
{
    public struct SpriteInstanceData
    {
        public static readonly uint StructSize = (uint)Unsafe.SizeOf<SpriteInstanceData>();

        public static SpriteInstanceData CopyFlags(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags);
        public static SpriteInstanceData TopLeft(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags | SpriteFlags.LeftAligned);
        public static SpriteInstanceData MidLeft(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags | SpriteFlags.LeftAligned | SpriteFlags.MidAligned);
        public static SpriteInstanceData BottomLeft(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags | SpriteFlags.LeftAligned | SpriteFlags.BottomAligned);
        public static SpriteInstanceData TopMid(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags);
        public static SpriteInstanceData Centred(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags | SpriteFlags.MidAligned);
        public static SpriteInstanceData BottomMid(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags) => new SpriteInstanceData(position, size, texPosition, texSize, texLayer, flags | SpriteFlags.BottomAligned);

        SpriteInstanceData(Vector3 position, Vector2 size, Vector2 texPosition, Vector2 texSize, uint texLayer, SpriteFlags flags)
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

            var transform = Matrix4x4.CreateTranslation(offset);

            if (flags.HasFlag(SpriteFlags.Floor)) 
            {
                transform *= new Matrix4x4(
                    1, 0, 0, 0,
                    0, 0,-1, 0,
                    0, 1, 0, 0,
                    0, 0, 0, 1);
            }

            transform *= Matrix4x4.CreateScale(new Vector3(size.X, size.Y, size.X));
            transform *= Matrix4x4.CreateTranslation(position);

            Transform1 = new Vector3(transform.M11, transform.M12, transform.M13);
            Transform2 = new Vector3(transform.M21, transform.M22, transform.M23);
            Transform3 = new Vector3(transform.M31, transform.M32, transform.M33);
            Transform4 = new Vector3(transform.M41, transform.M42, transform.M43);
            // Assume right column is always 0,0,0,1

            TexPosition = texPosition;
            TexSize = texSize;
            TexLayer = texLayer;
            Flags = flags;
        }

        public void OffsetBy(Vector3 offset)
        {
            Transform4 += offset;
        }

        public Vector3 Position => Transform4;

        public Matrix4x4 Transform => new Matrix4x4(
            Transform1.X, Transform1.Y, Transform1.Z, 0,
            Transform2.X, Transform2.Y, Transform2.Z, 0,
            Transform3.X, Transform3.Y, Transform3.Z, 0,
            Transform4.X, Transform4.Y, Transform4.Z, 1);

        public Vector3 Transform1 { get; private set; }
        public Vector3 Transform2 { get; private set; }
        public Vector3 Transform3 { get; private set; }
        public Vector3 Transform4 { get; private set; }
        public Vector2 TexPosition { get; private set; } // Normalised texture coordinates
        public Vector2 TexSize { get; private set; } // Normalised texture coordinates
        public uint TexLayer { get; private set; }
        public SpriteFlags Flags { get; set; }
    }
}
