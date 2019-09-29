using System;
using System.Collections.Generic;
using System.Linq;

namespace UAlbion.Api
{
    public static class Util
    {
        public static long LCM(IEnumerable<long> numbers) => numbers.Aggregate(LCM);
        public static long LCM(long a, long b) => Math.Abs(a * b) / GCD(a, b);
        public static long GCD(long a, long b) => b == 0 ? a : GCD(b, a % b);
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
    }
}
