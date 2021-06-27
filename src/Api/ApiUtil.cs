﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace UAlbion.Api
{
    public static class ApiUtil
    {
        /// <summary>
        /// Linearly interpolate between two values.
        /// </summary>
        /// <param name="a">The value to return when t is 0</param>
        /// <param name="b">The value to return when t is 1</param>
        /// <param name="t">The interpolation factor</param>
        /// <returns></returns>
        public static float Lerp(float a, float b, float t) => t * (b - a) + a;

        /// <summary>
        /// Convert the given angle in degrees to radians.
        /// </summary>
        /// <param name="degrees"></param>
        /// <returns></returns>
        public static float DegToRad(float degrees) => (float)Math.PI * degrees / 180.0f;

        /// <summary>
        /// Convert the given angle in radians to degrees.
        /// </summary>
        /// <param name="radians"></param>
        /// <returns></returns>
        public static float RadToDeg(float radians) => 180.0f * radians / (float)Math.PI;

        /// <summary>
        /// Calculate the lowest common multiple of the given numbers.
        /// </summary>
        /// <param name="numbers"></param>
        /// <returns></returns>
        public static long Lcm(IEnumerable<long> numbers) => numbers.Aggregate(1L, Lcm);

        /// <summary>
        /// Calculate the lowest common multiple of two numbers.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long Lcm(long a, long b) => Math.Abs(a * b) / Gcd(a, b);

        /// <summary>
        /// Calculate the greatest common denominator of two numbers.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static long Gcd(long a, long b) => b == 0 ? a : Gcd(b, a % b);

        /// <summary>
        /// Find the nearest power of two that is greater than or equal to the given value.
        /// </summary>
        public static int NextPowerOfTwo(int x) => (int)Math.Pow(2.0, Math.Ceiling(Math.Log(x, 2.0)));

        /// <summary>
        /// Transposes the given 8-bit image, swapping X and Y
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void TransposeImage<T>(int width, int height, ReadOnlySpan<T> from, Span<T> to)
        {
            if (to.Length < width * height)
                throw new ArgumentOutOfRangeException(
                    $"Tried to transpose a {width}x{height} image, but the source buffer does not contain enough bytes ({from.Length} < {width * height})");
            if (to.Length < width * height)
                throw new ArgumentOutOfRangeException(
                    $"Tried to transpose a {width}x{height} image, but the destination buffer does not contain enough bytes ({to.Length} < {width * height})");

            int transposedFrameHeight = width;

            int x = 0;
            int y = 0;
            for (int i = 0; i < width * height; i++)
            {
                int destIndex = y * height + x;
                to[destIndex] = from[i];

                y++;
                if (y == transposedFrameHeight)
                {
                    y = 0;
                    x++;
                }
            }
        }

        public static uint SizeInBytes<T>(this T[] array) where T : struct
        {
            if (array == null) throw new ArgumentNullException(nameof(array));
            return (uint)(array.Length * Unsafe.SizeOf<T>());
        }

        public static Matrix4x4 Inverse(this Matrix4x4 src)
        {
            Matrix4x4.Invert(src, out Matrix4x4 result);
            return result;
        }

        public static void Assert(string message)
        {
            #if DEBUG
            var oldColour = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.WriteLine("Assertion failed! " + message);
            Console.ForegroundColor = oldColour;
            #endif
            CoreTrace.Log.AssertFailed(message);
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Globalization", "CA1303:Do not pass literals as localized parameters", Justification = "Don't care")]
        public static void Assert(bool condition)
        {
            if (!condition)
                Assert("Assertion failed!");
        }

        public static void Assert(bool condition, string message)
        {
            if (!condition)
                Assert($"Assertion failed! {message}");
        }

        public static bool IsFlagsEnum(Type type) => type is {IsEnum: true} && type.GetCustomAttribute(typeof(FlagsAttribute)) != null;

        public static (byte r, byte g, byte b, byte a) UnpackColor(uint c)
        {
            var r = (byte)(c & 0xff);
            var g = (byte)((c >> 8) & 0xff);
            var b = (byte)((c >> 16) & 0xff);
            var a = (byte)((c >> 24) & 0xff);
            return (r, g, b, a);
        }

        public static uint PackColor(byte r, byte g, byte b, byte a) =>
            r
            | (uint)(g << 8)
            | (uint)(b << 16)
            | (uint)(a << 24);
    }
}
