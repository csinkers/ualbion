using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    public static class CoreUtil
    {
        public static bool IsDebug =>
#if DEBUG
            true;
#else
            false;
#endif

        public static Matrix4x4 CreatePerspective(
            bool isClipSpaceYInverted,
            bool useReverseDepth,
            float fov,
            float aspectRatio,
            float near, float far)
        {
            var persp = useReverseDepth
                ? CreatePerspective(fov, aspectRatio, far, near)
                : CreatePerspective(fov, aspectRatio, near, far);

            if (isClipSpaceYInverted)
            {
                persp *= new Matrix4x4(
                    1, 0, 0, 0,
                    0, -1, 0, 0,
                    0, 0, 1, 0,
                    0, 0, 0, 1);
            }

            return persp;
        }
        
        public static Matrix4x4 CreateLegacyPerspective(
            bool isClipSpaceYInverted,
            bool useReverseDepth,
            float fov,
            float aspectRatio,
            float near, float far,
            float pitch)
        {
            var persp = CreatePerspective(isClipSpaceYInverted, useReverseDepth, fov, aspectRatio, near, far);

            persp *= new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, -MathF.Tan(pitch) / MathF.Tan(fov * 0.5f), 0, 1);

            return persp;
        }

        static Matrix4x4 CreatePerspective(float fov, float aspectRatio, float near, float far)
        {
            if (fov <= 0.0f || fov >= MathF.PI)
                throw new ArgumentOutOfRangeException(nameof(fov));

            if (near <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(near));

            if (far <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(far));

            float yScale = 1.0f / MathF.Tan(fov * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4x4 result;

            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;

            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;

            result.M31 = result.M32 = 0.0f;
            var negFarRange = float.IsPositiveInfinity(far) ? -1.0f : far / (near - far);
            result.M33 = negFarRange;
            result.M34 = -1.0f;

            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = near * negFarRange;

            return result;
        }

        public static float[] GetFullScreenQuadVerts(bool isClipSpaceYInverted) =>
            isClipSpaceYInverted
                ? new float[]
                {
                    -1, -1, 0, 0,
                     1, -1, 1, 0,
                     1,  1, 1, 1,
                    -1,  1, 0, 1
                }
                : new float[]
                {
                    -1,  1, 0, 0,
                     1,  1, 1, 0,
                     1, -1, 1, 1,
                    -1, -1, 0, 1
                };

        public static ITexture BuildRotatedTexture(ICoreFactory factory, EightBitTexture texture)
        {
            if (factory == null) throw new ArgumentNullException(nameof(factory));
            if (texture == null) throw new ArgumentNullException(nameof(texture));
            var rotatedPixels = new byte[texture.Width * texture.Height];
            ApiUtil.RotateImage((int)texture.Width, (int)texture.Height,
               texture.TextureData,
               new Span<byte>(rotatedPixels));

            return factory.CreateEightBitTexture(
                texture.Name + "Rotated",
                texture.Height, texture.Width,
                texture.MipLevels, texture.ArrayLayers,
                rotatedPixels,
                new[] { new SubImage(
                    Vector2.Zero,
                    new Vector2(texture.Height, texture.Width),
                    new Vector2(texture.Height, texture.Width),
                    0)
                });
        }

        public static uint UpdateFlag(uint flags, FlagOperation operation, uint flag)
        {
            switch(operation)
            {
                case FlagOperation.Set: return flags | flag;
                case FlagOperation.Clear: return flags & ~flag;
                case FlagOperation.Toggle: return flags ^ flag;
                default: return flags;
            }
        }

        public static float UpdateValue(float value, ValueOperation operation, float argument)
        {
            switch(operation)
            {
                case ValueOperation.Set: return argument;
                case ValueOperation.Add: return value + argument;
                case ValueOperation.Mult: return value * argument;
                default: return value;
            }
        }

        static void Blit8To32Transparent(ReadOnlyByteImageBuffer fromBuffer, UIntImageBuffer toBuffer, uint[] palette, byte componentAlpha, byte transparentColor)
        {
            var from = fromBuffer.Buffer;
            var to = toBuffer.Buffer;
            int fromOffset = 0;
            int toOffset = 0;

            for (int j = 0; j < fromBuffer.Height; j++)
            {
                for (int i = 0; i < fromBuffer.Width; i++)
                {
                    uint pixel = from[fromOffset];
                    if (pixel != transparentColor)
                        to[toOffset] = palette[pixel] & 0x00ffffff | ((uint)componentAlpha << 24);

                    fromOffset++;
                    toOffset++;
                }

                fromOffset += (int)(fromBuffer.Stride - fromBuffer.Width);
                toOffset += (int)(toBuffer.Stride - toBuffer.Width);
            }
        }

        static void Blit8To32Opaque(ReadOnlyByteImageBuffer fromBuffer, UIntImageBuffer toBuffer, uint[] palette, byte componentAlpha)
        {
            var from = fromBuffer.Buffer;
            var to = toBuffer.Buffer;
            int fromOffset = 0;
            int toOffset = 0;

            for (int j = 0; j < fromBuffer.Height; j++)
            {
                for (int i = 0; i < fromBuffer.Width; i++)
                {
                    to[toOffset] = palette[from[fromOffset]] & 0x00ffffff | ((uint) componentAlpha << 24);
                    fromOffset++;
                    toOffset++;
                }

                fromOffset += (int)(fromBuffer.Stride - fromBuffer.Width);
                toOffset += (int)(toBuffer.Stride - toBuffer.Width);
            }
        }

        internal static void Blit8To32(ReadOnlyByteImageBuffer from, UIntImageBuffer to, uint[] palette, byte componentAlpha, byte? transparentColor)
        {
            uint remainingWidth = to.Width;
            uint remainingHeight = to.Height;
            Span<uint> dest = to.Buffer;

            uint chunkHeight = Math.Min(from.Height, to.Height);
            do
            {
                Span<uint> rowStart = dest;
                chunkHeight = Math.Min(chunkHeight, remainingHeight);
                uint chunkWidth = Math.Min(from.Width, to.Width);
                do
                {
                    chunkWidth = Math.Min(chunkWidth, remainingWidth);
                    var newFrom = new ReadOnlyByteImageBuffer(chunkWidth, chunkHeight, from.Stride, from.Buffer);
                    var newTo = new UIntImageBuffer(chunkWidth, chunkHeight, to.Stride, dest);

                    if (transparentColor.HasValue)
                        Blit8To32Transparent(newFrom, newTo, palette, componentAlpha, transparentColor.Value);
                    else
                        Blit8To32Opaque(newFrom, newTo, palette, componentAlpha);

                    dest = dest.Slice((int)chunkWidth);
                    remainingWidth -= chunkWidth;
                } while (remainingWidth > 0);

                remainingHeight -= chunkHeight;
                remainingWidth = to.Width;
                if (remainingHeight > 0)
                    dest = rowStart.Slice((int)(chunkHeight * to.Stride));
            } while (remainingHeight > 0);
        }

        public static void LogInfo(string msg) => Engine.GlobalExchange?.Raise(new LogEvent(LogEvent.Level.Info, msg), null);
        public static void LogWarn(string msg) => Engine.GlobalExchange?.Raise(new LogEvent(LogEvent.Level.Warning, msg), null);
        public static void LogError(string msg) => Engine.GlobalExchange?.Raise(new LogEvent(LogEvent.Level.Error, msg), null);
        public static void LogCritical(string msg) => Engine.GlobalExchange?.Raise(new LogEvent(LogEvent.Level.Critical, msg), null);
    }
}
