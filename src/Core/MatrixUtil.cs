using System;
using System.Numerics;

namespace UAlbion.Core
{
    public static class MatrixUtil
    {
        public static Vector4[] Print(this Matrix4x4 m) =>
            new[]
            {
                new Vector4(m.M11, m.M12, m.M13, m.M14),
                new Vector4(m.M21, m.M22, m.M23, m.M24),
                new Vector4(m.M31, m.M32, m.M33, m.M34),
                new Vector4(m.M41, m.M42, m.M43, m.M44),
            };

        public static Matrix4x4 CreatePerspective(
            bool isClipSpaceYInverted,
            bool useReverseDepth,
            bool depthZeroToOne,
            float fov,
            float aspectRatio,
            float near, float far)
        {
            var persp = useReverseDepth
                ? CreatePerspective(depthZeroToOne, fov, aspectRatio, far, near)
                : CreatePerspective(depthZeroToOne, fov, aspectRatio, near, far);

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
            bool depthZeroToOne,
            float fov,
            float aspectRatio,
            float near, float far,
            float pitch)
        {
            var persp = CreatePerspective(isClipSpaceYInverted, useReverseDepth, depthZeroToOne, fov, aspectRatio, near, far);

            persp *= new Matrix4x4(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                0, -MathF.Tan(pitch) / MathF.Tan(fov * 0.5f), 0, 1);

            return persp;
        }

        static Matrix4x4 CreatePerspective(bool depthZeroToOne, float fov, float aspectRatio, float near, float far)
        {
            if (fov <= 0.0f || fov >= MathF.PI) throw new ArgumentOutOfRangeException(nameof(fov));
            if (near <= 0.0f) throw new ArgumentOutOfRangeException(nameof(near));
            if (far <= 0.0f) throw new ArgumentOutOfRangeException(nameof(far));

            float yScale = 1.0f / MathF.Tan(fov * 0.5f);
            float xScale = yScale / aspectRatio;
            var negFarRange = float.IsPositiveInfinity(far) ? -1.0f : far / (near - far);

            if (depthZeroToOne)
            {
                return new Matrix4x4(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, negFarRange, -1.0f,
                    0, 0, near * negFarRange, 0);
            }
            else
            {
                // TODO
                return new Matrix4x4(
                    xScale, 0, 0, 0,
                    0, yScale, 0, 0,
                    0, 0, negFarRange, -1.0f,
                    0, 0, near * negFarRange, 0);
            }

/*
    z' = z * m22 + 1 * m23
    For [0..1]
    z'(f) = f  * m22 + m23 = 1
    z'(n) = n * m22 + m23 = 0

1 0  0 0
0 1  0 0
0 0 -1 0
0 0  1 0

    Diff: (f - n) m22 = 1 => m22 = 1/(f-n)
    m23 = n / (f-n) + m23 = 0 => m23 = -n/(f-n)

    For [-1..1]
*/
        }
    }
}