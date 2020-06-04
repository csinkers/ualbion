﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
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
        public static long Lcm(IEnumerable<long> numbers) => numbers.Aggregate(Lcm);

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
        /// Rotate the given 8-bit image 90 degrees clockwise.
        /// </summary>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public static void RotateImage(int width, int height, Span<byte> from, Span<byte> to)
        {
            if (to.Length < width * height)
                throw new ArgumentOutOfRangeException(
                    $"Tried to rotate a {width}x{height} image, but the source buffer does not contain enough bytes ({from.Length} < {width * height})");
            if (to.Length < width * height)
                throw new ArgumentOutOfRangeException(
                    $"Tried to rotate a {width}x{height} image, but the destination buffer does not contain enough bytes ({to.Length} < {width * height})");

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
            => (uint)(array.Length * Unsafe.SizeOf<T>());

        public static Matrix4x4 Inverse(this Matrix4x4 src)
        {
            Matrix4x4.Invert(src, out Matrix4x4 result);
            return result;
        }

        public static void Assert(string message)
        {
            #if DEBUG
            Console.WriteLine("Assertion failed! " + message);
            #endif
            CoreTrace.Log.AssertFailed(message);
        }

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
    }
}
