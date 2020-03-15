using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace UAlbion.Api
{
    public static class ApiUtil
    {
        public static float Lerp(float a, float b, float t) => t * (b - a) + a;
        public static float DegToRad(float degrees) => (float)Math.PI * degrees / 180.0f;
        public static float RadToDeg(float radians) => 180.0f * radians / (float)Math.PI;
        public static long Lcm(IEnumerable<long> numbers) => numbers.Aggregate(Lcm);
        public static long Lcm(long a, long b) => Math.Abs(a * b) / Gcd(a, b);
        public static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);
        public static void RotateImage(int width, int height, Span<byte> from, Span<byte> to)
        {
            int rotatedFrameHeight = width;

            int x = 0;
            int y = 0;
            for (int i = 0; i < width * height; i++)
            {
                int destIndex = y * height + x;
                to[destIndex] = from[i];

                y++;
                if (y == rotatedFrameHeight)
                {
                    y = 0;
                    x++;
                }
            }
        }

        public static uint SizeInBytes<T>(this T[] array) where T : struct
        {
            return (uint)(array.Length * Unsafe.SizeOf<T>());
        }

        public static Matrix4x4 Inverse(this Matrix4x4 src)
        {
            Matrix4x4.Invert(src, out Matrix4x4 result);
            return result;
        }

        public static void Assert(string message)
        {
            Console.Write("Assertion failed!");
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition)
        {
            if (!condition)
                Assert("Assertion failed!");
        }

        [Conditional("DEBUG")]
        public static void Assert(bool condition, string message)
        {
            if (!condition)
                Assert($"Assertion failed! {message}");
        }
    }
}
