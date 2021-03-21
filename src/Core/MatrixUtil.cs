using System;
using System.Numerics;

namespace UAlbion.Core
{
    public static class MatrixUtil
    {
        public static Vector4[] Print(this Matrix4x4 m) =>
            new []
            {
                new Vector4(m.M11, m.M12, m.M13, m.M14),
                new Vector4(m.M21, m.M22, m.M23, m.M24),
                new Vector4(m.M31, m.M32, m.M33, m.M34),
                new Vector4(m.M41, m.M42, m.M43, m.M44),
            };

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
            if (fov <= 0.0f || fov >= MathF.PI) throw new ArgumentOutOfRangeException(nameof(fov));
            if (near <= 0.0f) throw new ArgumentOutOfRangeException(nameof(near));
            if (far <= 0.0f) throw new ArgumentOutOfRangeException(nameof(far));

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
    }
}