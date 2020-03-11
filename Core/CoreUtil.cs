using System;
using System.Numerics;
using UAlbion.Api;
using UAlbion.Core.Textures;

namespace UAlbion.Core
{
    public static class CoreUtil
    {
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
                    1, 1, 1, 1,
                    -1, 1, 0, 1
                }
                : new float[]
                {
                    -1, 1, 0, 0,
                    1, 1, 1, 0,
                    1, -1, 1, 1,
                    -1, -1, 0, 1
                };

        public static ITexture BuildRotatedTexture(ICoreFactory factory, EightBitTexture texture)
        {
            var rotatedPixels = new byte[texture.Width * texture.Height];
            ApiUtil.RotateImage((int)texture.Width, (int)texture.Height, 
               new Span<byte>(texture.TextureData),
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

        static unsafe void Blit8To32Transparent(
            uint width, uint height,
            byte* from, uint* to,
            int fromStride, int toStride,
            uint[] palette, byte? transparentColor)
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    if (*from != transparentColor)
                        *to = palette[*from];

                    from++;
                    to++;
                }

                from += fromStride - width;
                to += toStride - width;
            }
        }

        static unsafe void Blit8To32Opaque(
            uint width, uint height,
            byte* from, uint* to,
            int fromStride, int toStride,
            uint[] palette)
        {
            for (int j = 0; j < height; j++)
            {
                for (int i = 0; i < width; i++)
                {
                    *to = palette[*from];
                    from++;
                    to++;
                }

                from += fromStride - width;
                to += toStride - width;
            }
        }

        internal static unsafe void Blit8To32(
            uint fromWidth, uint fromHeight, 
            uint toWidth, uint toHeight,
            byte* fromBuffer, uint* toBuffer, 
            int fromStride, int toStride,
            uint[] palette, byte? transparentColor)
        {
            uint initialToWidth = toWidth;
            int y = 0;
            do
            {
                int x = 0;
                uint height = Math.Min(fromHeight, toHeight);
                uint* rowStart = toBuffer;
                do
                {
                    uint width = Math.Min(fromWidth, toWidth);

                    if (transparentColor.HasValue)
                        Blit8To32Transparent(
                            width, height,
                            fromBuffer, toBuffer,
                            fromStride, toStride,
                            palette, transparentColor.Value);
                    else
                        Blit8To32Opaque(
                            width, height,
                            fromBuffer, toBuffer,
                            fromStride, toStride,
                            palette);

                    toBuffer += width;
                    toWidth -= width;
                    x++;
                } while (toWidth > 0);

                toBuffer = rowStart + height * toStride;
                toHeight -= height;
                toWidth = initialToWidth;
                y++;
            } while (toHeight > 0);
        }

        public static void LogInfo(string msg) => Engine.Global?.Raise(new LogEvent(LogEvent.Level.Info, msg), null);
        public static void LogWarn(string msg) => Engine.Global?.Raise(new LogEvent(LogEvent.Level.Warning, msg), null);
        public static void LogError(string msg) => Engine.Global?.Raise(new LogEvent(LogEvent.Level.Error, msg), null);
        public static void LogCritical(string msg) => Engine.Global?.Raise(new LogEvent(LogEvent.Level.Critical, msg), null);
    }
}
